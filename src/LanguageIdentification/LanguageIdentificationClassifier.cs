using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using LanguageIdentification.Internal;

namespace LanguageIdentification;

/// <summary>
/// <inheritdoc cref="ILanguageIdentificationClassifier"/>的默认实现
/// </summary>
public sealed class LanguageIdentificationClassifier : ILanguageIdentificationClassifier
{
    #region Private 字段

    private static readonly Encoding s_encoding = Encoding.UTF8;

    private readonly FixedSizeArrayPool<float> _confidenceArrayPool;
    private readonly ConfidenceCounter _counter;
    private readonly LanguageIdentificationModel _model;

    #endregion Private 字段

    #region Public 构造函数

    /// <summary>
    /// <inheritdoc cref="LanguageIdentificationClassifier"/><para/>
    /// 使用默认模型初始化
    /// </summary>
    public LanguageIdentificationClassifier() : this(LanguageIdentificationModel.Default)
    {
    }

    /// <inheritdoc cref="LanguageIdentificationClassifier(IEnumerable{string})"/>
    public LanguageIdentificationClassifier(params string[] languageCodes) : this(languageCodes as IEnumerable<string>)
    {
    }

    /// <summary>
    /// <inheritdoc cref="LanguageIdentificationClassifier"/><para/>
    /// 创建仅支持 <paramref name="languageCodes"/> 中的语言的分类器
    /// </summary>
    /// <param name="languageCodes">语言代码列表</param>
    public LanguageIdentificationClassifier(IEnumerable<string> languageCodes) : this(LanguageIdentificationModel.Create(languageCodes))
    {
    }

    /// <summary>
    /// <inheritdoc cref="LanguageIdentificationClassifier"/>
    /// </summary>
    /// <param name="model"></param>
    public LanguageIdentificationClassifier(LanguageIdentificationModel model)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _confidenceArrayPool = new(model.ClassesCount, Environment.ProcessorCount);
        _counter = new ConfidenceCounter(_confidenceArrayPool, model);
    }

    #endregion Public 构造函数

    #region Append

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(string text) => Append(text.AsSpan());

    /// <inheritdoc/>
    public void Append(ReadOnlySpan<char> text)
    {
        var byteBuffer = ArrayPool<byte>.Shared.Rent(text.Length * 4);
        try
        {
            Span<byte> buffer = byteBuffer;

#if NETSTANDARD2_0_OR_GREATER
            var length = s_encoding.GetBytes(text.ToArray(), 0, text.Length, byteBuffer, 0);
#else
            var length = s_encoding.GetBytes(text, buffer);
#endif

            Append(buffer.Slice(0, length));
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(byteBuffer);
        }
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(byte[] buffer, int start, int length) => Append(new ReadOnlySpan<byte>(buffer, start, length));

    /// <inheritdoc/>
    public void Append(ReadOnlySpan<byte> buffer)
    {
        var length = buffer.Length;

        // Update predictions (without an intermediate statecount as in the original)
        short state = 0;
        var tk_output = _model.dsaOutput;
        var tk_nextmove = _model.dsa;

        for (var i = 0; i < length; i++)
        {
            state = tk_nextmove[(state << 8) + (buffer[i] & byte.MaxValue)];

            var output = tk_output[state];
            if (output is not null)
            {
                foreach (var feature in output)
                {
                    _counter.Increment(feature);
                }
            }
        }
    }

    #endregion Append

    #region Public 方法

    /// <inheritdoc/>
    public ILanguageDetectionResult Classify()
    {
        var confidences = _counter.NaiveBayesClassConfidence();

        var index = 0;
        var max = confidences[index];

        for (var i = 1; i < confidences.Length; i++)
        {
            if (confidences[i] > max)
            {
                index = i;
                max = confidences[i];
            }
        }

        return new PooledConfidenceLanguageDetectionResult(_model.LanguageClasses[index], index, confidences, _confidenceArrayPool);
    }

    /// <inheritdoc/>
    public ILanguageDetectionResultRank CreateRank()
    {
        var confidences = _counter.NaiveBayesClassConfidence();
        return new LanguageDetectionResultRank(_model.LanguageClasses, confidences, _confidenceArrayPool);
    }

    /// <summary>
    /// 获取所有支持的语言
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> GetSupportedLanguages() => _model.GetSupportedLanguages();

    /// <inheritdoc/>
    public void Reset()
    {
        _counter.Clear();
    }

    #endregion Public 方法

    #region Static

    #region Classify

    /// <summary>
    /// 分类并获取 <paramref name="text"/> 最高可能性的语言
    /// </summary>
    /// <param name="text">需要识别的文本数据</param>
    /// <returns></returns>
    public static ILanguageDetectionResult Classify(string text) => Classify(text.AsSpan());

    /// <summary>
    /// 分类并获取 <paramref name="text"/> 最高可能性的语言
    /// </summary>
    /// <param name="text">需要识别的文本数据</param>
    /// <returns></returns>
    public static ILanguageDetectionResult Classify(ReadOnlySpan<char> text)
    {
        var classifier = LanguageIdentificationClassifierPool.Default.Rent();
        try
        {
            classifier.Append(text);
            return classifier.Classify();
        }
        finally
        {
            LanguageIdentificationClassifierPool.Default.Return(classifier);
        }
    }

    /// <summary>
    /// 分类并获取 <paramref name="buffer"/> 最高可能性的语言
    /// </summary>
    /// <param name="buffer">文本的UTF8编码数据buffer</param>
    /// <param name="start"><paramref name="buffer"/>中的起始索引</param>
    /// <param name="length"><paramref name="buffer"/>中的数据长度</param>
    /// <returns></returns>
    public static ILanguageDetectionResult Classify(byte[] buffer, int start, int length) => Classify(new ReadOnlySpan<byte>(buffer, start, length));

    /// <summary>
    /// 分类并获取 <paramref name="buffer"/> 最高可能性的语言
    /// </summary>
    /// <param name="buffer">文本的UTF8编码数据</param>
    /// <returns></returns>
    public static ILanguageDetectionResult Classify(ReadOnlySpan<byte> buffer)
    {
        var classifier = LanguageIdentificationClassifierPool.Default.Rent();
        try
        {
            classifier.Append(buffer);
            return classifier.Classify();
        }
        finally
        {
            LanguageIdentificationClassifierPool.Default.Return(classifier);
        }
    }

    #endregion Classify

    #region CreateRank

    /// <summary>
    /// 分类并获取 <paramref name="text"/> 结果
    /// </summary>
    /// <param name="text">需要识别的文本数据</param>
    /// <returns></returns>
    public static ILanguageDetectionResultRank CreateRank(string text) => CreateRank(text.AsSpan());

    /// <summary>
    /// 分类并获取 <paramref name="text"/> 结果
    /// </summary>
    /// <param name="text">需要识别的文本数据</param>
    /// <returns></returns>
    public static ILanguageDetectionResultRank CreateRank(ReadOnlySpan<char> text)
    {
        var classifier = LanguageIdentificationClassifierPool.Default.Rent();
        try
        {
            classifier.Append(text);
            return classifier.CreateRank();
        }
        finally
        {
            LanguageIdentificationClassifierPool.Default.Return(classifier);
        }
    }

    /// <summary>
    /// 分类并获取 <paramref name="buffer"/> 结果
    /// </summary>
    /// <param name="buffer">文本的UTF8编码数据buffer</param>
    /// <param name="start"><paramref name="buffer"/>中的起始索引</param>
    /// <param name="length"><paramref name="buffer"/>中的数据长度</param>
    /// <returns></returns>
    public static ILanguageDetectionResultRank CreateRank(byte[] buffer, int start, int length) => CreateRank(new ReadOnlySpan<byte>(buffer, start, length));

    /// <summary>
    /// 分类并获取 <paramref name="buffer"/> 结果
    /// </summary>
    /// <param name="buffer">文本的UTF8编码数据</param>
    /// <returns></returns>
    public static ILanguageDetectionResultRank CreateRank(ReadOnlySpan<byte> buffer)
    {
        var classifier = LanguageIdentificationClassifierPool.Default.Rent();
        try
        {
            classifier.Append(buffer);
            return classifier.CreateRank();
        }
        finally
        {
            LanguageIdentificationClassifierPool.Default.Return(classifier);
        }
    }

    #endregion CreateRank

    /// <summary>
    /// 获取所有支持的语言
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<string> GetAllSupportedLanguages() => LanguageIdentificationModel.Default.GetSupportedLanguages();

    #endregion Static
}

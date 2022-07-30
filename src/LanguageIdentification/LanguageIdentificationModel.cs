using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

#pragma warning disable CS0659 // 类型重写 Object.Equals(object o)，但不重写 Object.GetHashCode()
#pragma warning disable IDE1006 // 命名样式

namespace LanguageIdentification;

/// <summary>
/// 语言检测模型
/// </summary>
public sealed class LanguageIdentificationModel : IEquatable<LanguageIdentificationModel>
{
    #region Internal 字段

    /// <summary>
    /// State machine for walking byte n-grams.
    /// </summary>
    internal short[] dsa;

    /// <summary>
    /// An output (may be null) associated with each state.
    /// </summary>
    internal int[]?[] dsaOutput;

    /// <summary>
    /// Conditional init per-language probabilities (?).
    /// </summary>
    internal float[] nb_pc;

    /// <summary>
    /// Flattened matrix of per-language feature probabilities.
    /// [featureIndex]
    /// [langIndex]
    /// where
    /// index = {@link #numClasses} * langIndex + featureIndex
    /// </summary>
    internal float[] nb_ptc;

    #endregion Internal 字段

    #region Private 字段

    /// <summary>
    /// The default model, initialized lazily (once).
    /// </summary>
    private static readonly WeakReference<LanguageIdentificationModel?> s_defaultModelReference = new(null);

    #endregion Private 字段

    #region Public 属性

    /// <summary>
    /// 默认模型
    /// </summary>
    public static LanguageIdentificationModel Default
    {
        get
        {
            if (s_defaultModelReference.TryGetTarget(out var defaultModel))
            {
                return defaultModel;
            }
            lock (s_defaultModelReference)
            {
                if (s_defaultModelReference.TryGetTarget(out defaultModel))
                {
                    return defaultModel;
                }
                defaultModel = InternalLoadDefaultModel();
                s_defaultModelReference.SetTarget(defaultModel);
            }
            return defaultModel;
        }
    }

    /// <summary>
    /// Number of classes (languages).
    /// </summary>
    public int ClassesCount { get; }

    /// <summary>
    /// Number of features (total).
    /// </summary>
    public int FeaturesCount { get; }

    #endregion Public 属性

    #region Internal 属性

    /// <summary>
    /// Language classes.
    /// </summary>
    internal string[] LanguageClasses { get; }

    #endregion Internal 属性

    #region Public 构造函数

    /// <summary>
    /// <inheritdoc cref="LanguageIdentificationModel"/><para/>
    /// 参数意义参见:<para/>
    /// https://github.com/carrotsearch/langid-java <para/>
    /// https://github.com/saffsd/langid.py
    /// </summary>
    /// <param name="langClasses"></param>
    /// <param name="ptc"></param>
    /// <param name="pc"></param>
    /// <param name="dsa"></param>
    /// <param name="dsaOutput"></param>
    public LanguageIdentificationModel(string[] langClasses, float[] ptc, float[] pc, short[] dsa, int[]?[] dsaOutput)
    {
        LanguageClasses = langClasses ?? throw new ArgumentNullException(nameof(langClasses));
        nb_ptc = ptc ?? throw new ArgumentNullException(nameof(ptc));
        nb_pc = pc ?? throw new ArgumentNullException(nameof(pc));
        this.dsa = dsa ?? throw new ArgumentNullException(nameof(dsa));
        this.dsaOutput = dsaOutput ?? throw new ArgumentNullException(nameof(dsaOutput));

        Debug.Assert(nb_pc.Length == langClasses.Length);
        this.ClassesCount = langClasses.Length;
        this.FeaturesCount = nb_ptc.Length / ClassesCount;
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc cref="Create(IEnumerable{string})"/>
    public static LanguageIdentificationModel Create(params string[] languageCodes) => Create(languageCodes as IEnumerable<string>);

    /// <summary>
    /// 创建仅支持指定语言的模型
    /// </summary>
    /// <param name="languageCodes">语言代码列表</param>
    /// <returns></returns>
    public static LanguageIdentificationModel Create(IEnumerable<string> languageCodes)
    {
        var originModel = Default;

        var inputLanguages = languageCodes.Distinct().ToArray();

        var languages = new HashSet<string>(originModel.LanguageClasses.Intersect(inputLanguages));

        if (languages.Count != inputLanguages.Length)
        {
            var errorLanguage = string.Join(", ", inputLanguages.Except(languages));
            throw new ArgumentException($"there is some languageCode not supported or error languageCode. {errorLanguage}");
        }

        if (languages.Count < 2)
        {
            throw new ArgumentException("A model must contain at least two languages.");
        }

        // Limit the set of supported languages (fewer languages = tighter loops and faster execution).
        float[] trimmed_nb_pc = new float[languages.Count];
        float[] trimmed_nb_ptc = new float[languages.Count * originModel.FeaturesCount];
        for (int i = 0, j = 0; i < originModel.ClassesCount; i++)
        {
            if (languages.Contains(originModel.LanguageClasses[i]))
            {
                trimmed_nb_pc[j] = originModel.nb_pc[i];
                for (int f = 0; f < originModel.FeaturesCount; f++)
                {
                    int iFrom = originModel.FeaturesCount * i + f;
                    int iTo = originModel.FeaturesCount * j + f;
                    trimmed_nb_ptc[iTo] = originModel.nb_ptc[iFrom];
                }
                j++;
            }
        }

        return new LanguageIdentificationModel(languages.ToArray(),
                               trimmed_nb_ptc,
                               trimmed_nb_pc,
                               originModel.dsa,
                               originModel.dsaOutput);
    }

    /// <summary>
    /// 从流中反序列化 <see cref="LanguageIdentificationModel"/> 对象
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static LanguageIdentificationModel Deserialize(Stream stream)
    {
        using var reader = new BinaryReader(stream, Encoding.UTF8, true);
        string[] langClasses = reader.ReadStringArray()!;
        var nb_ptc = reader.ReadFloatArray()!;
        var nb_pc = reader.ReadFloatArray()!;
        var dsa = reader.ReadInt16Array()!;

        int[]?[]? dsaOutput = null;
        if (reader.ReadArrayLength<int[]>(out var array) is int length
            && length > 0)
        {
            dsaOutput = new int[length][];
            for (int i = 0; i < length; i++)
            {
                dsaOutput[i] = reader.ReadInt32Array();
            }
        }
        else
        {
            dsaOutput = array;
        }

        return new LanguageIdentificationModel(langClasses, nb_ptc, nb_pc, dsa, dsaOutput ?? throw new SerializationException("dsaOutput deserialize fail."));
    }

    /// <summary>
    /// 获取所有支持的语言
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> GetSupportedLanguages() => LanguageClasses.AsEnumerable();

    /// <summary>
    /// 将当前模型序列化到流中
    /// </summary>
    /// <param name="stream"></param>
    public void Serialize(Stream stream)
    {
        using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
        writer.WriteStringArray(LanguageClasses);
        writer.WriteFloatArray(nb_ptc);
        writer.WriteFloatArray(nb_pc);
        writer.WriteInt16Array(dsa);

        if (writer.WriteArrayLength(dsaOutput))
        {
            foreach (var item in dsaOutput)
            {
                writer.WriteInt32Array(item);
            }
        }
    }

    #region Equals

    /// <inheritdoc/>
    public bool Equals(LanguageIdentificationModel? other)
    {
        return other is not null
                && other.ClassesCount == ClassesCount
                && SequenceEqual(other.dsa, dsa)
                && other.FeaturesCount == FeaturesCount
                && SequenceEqual(other.LanguageClasses, LanguageClasses)
                && SequenceEqual(other.nb_pc, nb_pc)
                && SequenceEqual(other.nb_ptc, nb_ptc)
                && other.dsaOutput is not null
                && dsaOutput is not null
                && Enumerable.SequenceEqual(other.dsaOutput, dsaOutput, new ArrayEqualityComparer<int>())
                ;

        static bool SequenceEqual<T>(T[]? array1, T[]? array2)
        {
            return array1 is not null
                   && array2 is not null
                   && Enumerable.SequenceEqual(array1, array2);
        }
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return this.Equals(obj as LanguageIdentificationModel);
    }

    private class ArrayEqualityComparer<T> : IEqualityComparer<T[]?>
    {
        #region Public 方法

        public bool Equals(T[]? x, T[]? y)
        {
            if (x is null && y is null)
            {
                return true;
            }

            return x != null
                   && y != null
                   && Enumerable.SequenceEqual(x, y);
        }

        public int GetHashCode(T[] obj)
        {
            var result = int.MaxValue;
            foreach (var item in obj)
            {
                result &= item?.GetHashCode() ?? 0;
            }
            return result;
        }

        #endregion Public 方法
    }

    #endregion Equals

    #endregion Public 方法

    #region Private 方法

    /// <summary>
    /// 加载默认模型
    /// </summary>
    /// <returns></returns>
    private static LanguageIdentificationModel InternalLoadDefaultModel()
    {
        using var stream = new MemoryStream(Resource.ModelData);
        using var gzipStream = new GZipStream(stream, CompressionMode.Decompress);
        using var bufferedStream = new BufferedStream(gzipStream);

        return Deserialize(bufferedStream);
    }

    #endregion Private 方法
}
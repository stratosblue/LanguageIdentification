using System;

namespace LanguageIdentification;

/// <summary>
/// 语言检测结果
/// </summary>
internal sealed class PooledConfidenceLanguageDetectionResult : ILanguageDetectionResult
{
    #region Private 字段

    private readonly IFixedSizeArrayPool<float> _confidenceArrayPool;
    private readonly float[] _confidences;
    private readonly int _index;
    private bool _isDisposed;
    private float? _normalizeConfidence;

    #endregion Private 字段

    #region Public 属性

    /// <inheritdoc/>
    public float Confidence { get; }

    /// <inheritdoc/>
    public string LanguageCode { get; }

    /// <inheritdoc/>
    public float NormalizeConfidence => _normalizeConfidence ?? GetConfidenceValue();

    #endregion Public 属性

    #region 构造函数

    /// <summary>
    /// <inheritdoc cref="LanguageDetectionResult"/>
    /// </summary>
    /// <param name="languageCode">语言Code</param>
    /// <param name="index">当前语言在<paramref name="confidences"/>的索引</param>
    /// <param name="confidences">所有置信度</param>
    /// <param name="confidenceArrayPool"></param>
    public PooledConfidenceLanguageDetectionResult(string languageCode, int index, float[] confidences, IFixedSizeArrayPool<float> confidenceArrayPool)
    {
        LanguageCode = languageCode ?? throw new ArgumentNullException(nameof(languageCode));
        _confidences = confidences ?? throw new ArgumentNullException(nameof(confidences));
        _confidenceArrayPool = confidenceArrayPool ?? throw new ArgumentNullException(nameof(confidenceArrayPool));

        _index = index;

        Confidence = confidences[index];
    }

    #endregion 构造函数

    #region Private 方法

    private float GetConfidenceValue()
    {
        ThrowIfDisposed();
        return (_normalizeConfidence = LanguageDetectionResult.NormalizeConfidenceAsProbability(_index, _confidences!)).Value;
    }

    #endregion Private 方法

    #region Public 方法

    /// <inheritdoc/>
    public override string ToString() => LanguageDetectionResult.ToString(this);

    #endregion Public 方法

    #region IDisposable

    /// <summary>
    ///
    /// </summary>
    ~PooledConfidenceLanguageDetectionResult()
    {
        InternalDispose();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        InternalDispose();
        GC.SuppressFinalize(this);
    }

    private void InternalDispose()
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
            if (_confidenceArrayPool is not null
                && _confidences is not null)
            {
                _confidenceArrayPool.Return(_confidences);
            }
        }
    }

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(LanguageDetectionResultRank));
        }
    }

    #endregion IDisposable
}

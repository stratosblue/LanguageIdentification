using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LanguageIdentification;

/// <summary>
/// 语言检测结果排行
/// </summary>
internal sealed class LanguageDetectionResultRank : ILanguageDetectionResultRank
{
    #region Private 字段

    private readonly IFixedSizeArrayPool<float>? _confidenceArrayPool;
    private readonly IEnumerable<ILanguageDetectionResult> _results;
    private bool _isDisposed;

    #endregion Private 字段

    #region Internal 属性

    internal float[] Confidences { get; }

    #endregion Internal 属性

    #region Public 属性

    /// <summary>
    /// 总数
    /// </summary>
    public int Count => Confidences.Length;

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc/>
    public LanguageDetectionResultRank(string[] languageClasses, float[] confidences, IFixedSizeArrayPool<float> confidenceArrayPool)
    {
        this.Confidences = confidences;
        _confidenceArrayPool = confidenceArrayPool;
        _results = confidences.Select((item, index) => new RankLanguageDetectionResult(this, languageClasses[index], index));
    }

    #endregion Public 构造函数

    #region Internal 方法

    internal float NormalizeConfidenceAsProbability(int index)
    {
        ThrowIfDisposed();
        return LanguageDetectionResult.NormalizeConfidenceAsProbability(index, Confidences);
    }

    #endregion Internal 方法

    #region Public 方法

    /// <inheritdoc/>
    public IEnumerator<ILanguageDetectionResult> GetEnumerator()
    {
        ThrowIfDisposed();
        return _results.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        ThrowIfDisposed();
        return this.GetEnumerator();
    }

    #endregion Public 方法

    #region IDisposable

    /// <summary>
    ///
    /// </summary>
    ~LanguageDetectionResultRank()
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
            _confidenceArrayPool?.Return(Confidences);
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

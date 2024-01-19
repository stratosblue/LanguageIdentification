using System;
using System.Collections.Generic;

namespace LanguageIdentification;

/// <summary>
/// 语言检测结果排行
/// </summary>
public interface ILanguageDetectionResultRank : IEnumerable<ILanguageDetectionResult>, IDisposable
{
    #region Public 属性

    /// <summary>
    /// 数量
    /// </summary>
    int Count { get; }

    #endregion Public 属性
}

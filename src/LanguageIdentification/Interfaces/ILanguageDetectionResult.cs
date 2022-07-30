using System;

namespace LanguageIdentification;

/// <summary>
/// 语言检测结果
/// </summary>
public interface ILanguageDetectionResult: IDisposable
{
    #region Public 属性

    /// <summary>
    /// 置信度（原始值）
    /// </summary>
    float Confidence { get; }

    /// <summary>
    /// 语言Code
    /// </summary>
    string LanguageCode { get; }

    /// <summary>
    /// 置信度（百分比）
    /// </summary>
    float NormalizeConfidence { get; }

    #endregion Public 属性
}
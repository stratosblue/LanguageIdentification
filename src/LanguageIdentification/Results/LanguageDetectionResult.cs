using System;

namespace LanguageIdentification
{
    /// <summary>
    /// 语言检测结果
    /// </summary>
    public sealed class LanguageDetectionResult : ILanguageDetectionResult
    {
        #region Public 属性

        /// <inheritdoc/>
        public float Confidence { get; }

        /// <inheritdoc/>
        public string LanguageCode { get; }

        /// <inheritdoc/>
        public float NormalizeConfidence { get; }

        #endregion Public 属性

        #region Public 构造函数

        /// <summary>
        /// <inheritdoc cref="LanguageDetectionResult"/>
        /// </summary>
        /// <param name="languageCode">语言Code</param>
        /// <param name="confidence">置信度</param>
        /// <param name="normalizeConfidence">置信度换算百分比</param>
        public LanguageDetectionResult(string languageCode, float confidence, float normalizeConfidence)
        {
            LanguageCode = languageCode ?? throw new ArgumentNullException(nameof(languageCode));
            Confidence = confidence;
            NormalizeConfidence = normalizeConfidence;
        }

        #endregion Public 构造函数

        #region Public 方法

        /// <summary>
        /// 计算置信度为百分比
        /// </summary>
        /// <param name="index">目标值在<paramref name="confidences"/>的索引</param>
        /// <param name="confidences">所有置信度</param>
        /// <returns></returns>
        public static float NormalizeConfidenceAsProbability(int index, float[] confidences)
        {
            // Renormalize log-probs into a proper distribution
            float s = 0;
            float v = confidences![index];
            for (int j = 0; j < confidences!.Length; j++)
            {
                s += (float)Math.Exp(confidences![j] - v);
            }
            return 1 / s;
        }

        /// <summary>
        /// 格式化输出
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static string ToString(ILanguageDetectionResult result)
        {
            return $"[{result.LanguageCode} - Confidence: {result.NormalizeConfidence}]";
        }

        /// <inheritdoc/>
        public void Dispose()
        { }

        /// <inheritdoc/>
        public override string ToString() => ToString(this);

        #endregion Public 方法
    }
}
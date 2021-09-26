using System;

namespace LanguageIdentification
{
    /// <summary>
    /// 语言检测结果
    /// </summary>
    public sealed class LanguageDetectionResult
    {
        #region Private 字段

        private readonly float[]? _confidences;
        private readonly int _index;
        private float? _normalizeConfidence;

        #endregion Private 字段

        #region Public 属性

        /// <summary>
        /// 置信度（原始值）
        /// </summary>
        public float Confidence { get; }

        /// <summary>
        /// 语言Code
        /// </summary>
        public string LanguageCode { get; }

        /// <summary>
        /// 置信度（百分比）
        /// </summary>
        public float NormalizeConfidence => _normalizeConfidence ?? (_normalizeConfidence = NormalizeConfidenceAsProbability()).Value;

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
            LanguageCode = languageCode;
            Confidence = confidence;
            _normalizeConfidence = normalizeConfidence;
        }

        /// <summary>
        /// <inheritdoc cref="LanguageDetectionResult"/>
        /// </summary>
        /// <param name="languageCode">语言Code</param>
        /// <param name="confidences">所有置信度</param>
        /// <param name="index">当前语言在<paramref name="confidences"/>的索引</param>
        public LanguageDetectionResult(string languageCode, float[] confidences, int index)
        {
            LanguageCode = languageCode;
            _confidences = confidences;
            _index = index;
            Confidence = confidences[index];
        }

        #endregion Public 构造函数

        #region Public 方法

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"[{LanguageCode} - Confidence: {NormalizeConfidence}]";
        }

        #endregion Public 方法

        #region Private 方法

        /// <summary>
        /// 计算置信度为百分比
        /// </summary>
        /// <returns></returns>
        private float NormalizeConfidenceAsProbability()
        {
            // Renormalize log-probs into a proper distribution
            float s = 0;
            float v = _confidences![_index];
            for (int j = 0; j < _confidences!.Length; j++)
            {
                s += (float)Math.Exp(_confidences![j] - v);
            }
            return 1 / s;
        }

        #endregion Private 方法
    }
}
using System;

namespace LanguageIdentification
{
    internal sealed class RankLanguageDetectionResult : ILanguageDetectionResult
    {
        #region Private 字段

        private readonly LanguageDetectionResultRank _detectionResultRank;
        private readonly int _index;
        private float? _normalizeConfidence;

        #endregion Private 字段

        #region Public 属性

        /// <inheritdoc/>
        public float Confidence { get; }

        /// <inheritdoc/>
        public string LanguageCode { get; }

        /// <inheritdoc/>
        public float NormalizeConfidence => _normalizeConfidence ?? (_normalizeConfidence = _detectionResultRank.NormalizeConfidenceAsProbability(_index)).Value;

        #endregion Public 属性

        #region Public 构造函数

        /// <summary>
        /// <inheritdoc cref="LanguageDetectionResult"/>
        /// </summary>
        /// <param name="detectionResultRank"></param>
        /// <param name="languageCode">语言Code</param>
        /// <param name="index"></param>
        public RankLanguageDetectionResult(LanguageDetectionResultRank detectionResultRank, string languageCode, int index)
        {
            _detectionResultRank = detectionResultRank ?? throw new ArgumentNullException(nameof(detectionResultRank));
            LanguageCode = languageCode ?? throw new ArgumentNullException(nameof(languageCode));
            Confidence = detectionResultRank.Confidences[index];
            _index = index;
        }

        #endregion Public 构造函数

        #region Public 方法

        /// <inheritdoc/>
        public void Dispose()
        { }

        /// <inheritdoc/>
        public override string ToString() => LanguageDetectionResult.ToString(this);

        #endregion Public 方法
    }
}
using System;
using System.Collections.Generic;

namespace LanguageIdentification
{
    /// <summary>
    /// 语言识别分类器
    /// </summary>
    public interface ILanguageIdentificationClassifier
    {
        #region Public 方法

        /// <summary>
        /// 追加文本到分类器
        /// </summary>
        /// <param name="text"></param>
        void Append(string text);

        /// <summary>
        /// 追加文本到分类器
        /// </summary>
        /// <param name="text"></param>
        void Append(ReadOnlySpan<char> text);

        /// <summary>
        /// 追加文本到分类器
        /// </summary>
        /// <param name="buffer">文本的UTF8编码数据buffer</param>
        /// <param name="start"><paramref name="buffer"/>中的起始索引</param>
        /// <param name="length"><paramref name="buffer"/>中的数据长度</param>
        void Append(byte[] buffer, int start, int length);

        /// <summary>
        /// 追加文本到分类器
        /// </summary>
        /// <param name="buffer">文本的UTF8编码数据</param>
        void Append(ReadOnlySpan<byte> buffer);

        /// <summary>
        /// 依据当前已添加到分类器的文本，分类并获取最高可能性的语言
        /// </summary>
        /// <returns></returns>
        LanguageDetectionResult Classify();

        /// <summary>
        /// 创建所有支持语言的置信度数据
        /// </summary>
        /// <returns>所有支持语言的置信度数据（未排序）</returns>
        IEnumerable<LanguageDetectionResult> CreateRank();

        /// <summary>
        /// 获取所有支持的语言
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetSupportedLanguages();

        /// <summary>
        /// 重置分类器数据，以重新使用
        /// </summary>
        void Reset();

        #endregion Public 方法
    }
}
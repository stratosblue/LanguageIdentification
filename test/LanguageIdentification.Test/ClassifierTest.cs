using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LanguageIdentification.Test
{
    [TestClass]
    public class LanguageIdentificationClassifierTest
    {
        #region Public 方法

        [TestMethod]
        public void ShouldBeSuccess()
        {
            var langIdClassifier = new LanguageIdentificationClassifier();

            foreach (var item in TestData.Items)
            {
                langIdClassifier.Append(item.Text);
                var result = langIdClassifier.Classify();
                Assert.AreEqual(item.LanguageCode, result.LanguageCode);

                Console.WriteLine(result);

                langIdClassifier.Reset();
            }
        }

        [TestMethod]
        public void ShouldCreateRankSuccess()
        {
            var langIdClassifier = new LanguageIdentificationClassifier();

            var supportedLanguages = langIdClassifier.GetSupportedLanguages().ToArray();

            foreach (var item in TestData.Items)
            {
                langIdClassifier.Append(item.Text);
                var result = langIdClassifier.Classify();

                var rank = langIdClassifier.CreateRank().ToArray();
                Assert.AreEqual(supportedLanguages.Length, rank.Length);

                langIdClassifier.Reset();
            }
        }

        #endregion Public 方法
    }
}
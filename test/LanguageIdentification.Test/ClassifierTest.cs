using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LanguageIdentification.Test
{
    [TestClass]
    public class LanguageIdentificationClassifierTest
    {
        #region Public 方法

        [TestMethod]
        public void CanBeEnumeratedRankManyTimes()
        {
            var langIdClassifier = new LanguageIdentificationClassifier();

            var supportedLanguageCount = langIdClassifier.GetSupportedLanguages().Count();

            foreach (var item in TestData.Items)
            {
                langIdClassifier.Append(item.Text);
                using var rank = langIdClassifier.CreateRank();

                Assert.AreEqual(supportedLanguageCount, rank.Count());
                Assert.AreEqual(supportedLanguageCount, rank.Count());
                Assert.AreEqual(supportedLanguageCount, rank.Count());
                Assert.AreEqual(supportedLanguageCount, rank.Count());

                langIdClassifier.Reset();
            }
        }

        [TestMethod]
        public void ShouldBeSuccess()
        {
            var langIdClassifier = new LanguageIdentificationClassifier();

            foreach (var item in TestData.Items)
            {
                langIdClassifier.Append(item.Text);
                using var result = langIdClassifier.Classify();
                Assert.AreEqual(item.LanguageCode, result.LanguageCode);

                Console.WriteLine(result);

                langIdClassifier.Reset();
            }
        }

        [TestMethod]
        public void ShouldBeSuccessAfterMultiClassify()
        {
            var langIdClassifier = new LanguageIdentificationClassifier();

            var results = new List<KeyValuePair<string, double>>();

            foreach (var item in TestData.Items)
            {
                langIdClassifier.Append(item.Text);
                using var result = langIdClassifier.Classify();
                Assert.AreEqual(item.LanguageCode, result.LanguageCode);

                results.Add(new(result.LanguageCode, result.NormalizeConfidence));

                langIdClassifier.Reset();
            }

            var mResults = new List<ILanguageDetectionResult>();

            foreach (var item in TestData.Items)
            {
                langIdClassifier.Append(item.Text);
                mResults.Add(langIdClassifier.Classify());
                langIdClassifier.Reset();
            }

            Assert.AreEqual(results.Count, mResults.Count);

            for (int i = 0; i < results.Count; i++)
            {
                Assert.AreEqual(results[i].Value, mResults[i].NormalizeConfidence);
            }
        }

        [TestMethod]
        public void ShouldCreateRankSuccess()
        {
            var langIdClassifier = new LanguageIdentificationClassifier();

            var supportedLanguageCount = langIdClassifier.GetSupportedLanguages().Count();

            foreach (var item in TestData.Items)
            {
                langIdClassifier.Append(item.Text);
                using var result = langIdClassifier.Classify();

                var rank = langIdClassifier.CreateRank().ToArray();
                Assert.AreEqual(supportedLanguageCount, rank.Length);

                langIdClassifier.Reset();
            }
        }

        [TestMethod]
        public void UnableAccessRankAfterDisposed()
        {
            var langIdClassifier = new LanguageIdentificationClassifier();

            foreach (var item in TestData.Items)
            {
                langIdClassifier.Append(item.Text);
                var rank = langIdClassifier.CreateRank();

                Assert.IsTrue(rank.Count() > 0);

                rank.Dispose();

                Assert.ThrowsException<ObjectDisposedException>(() => rank.Count());

                rank = langIdClassifier.CreateRank();

                using var rankItem = rank.First();
                rank.Dispose();

                Assert.ThrowsException<ObjectDisposedException>(() => rankItem.NormalizeConfidence);

                langIdClassifier.Reset();
            }
        }

        [TestMethod]
        public void UnableAccessResultAfterDisposed()
        {
            var langIdClassifier = new LanguageIdentificationClassifier();

            foreach (var item in TestData.Items)
            {
                langIdClassifier.Append(item.Text);
                var result = langIdClassifier.Classify();

                result.Dispose();

                Assert.ThrowsException<ObjectDisposedException>(() => result.NormalizeConfidence);

                langIdClassifier.Reset();
            }
        }

        #endregion Public 方法
    }
}
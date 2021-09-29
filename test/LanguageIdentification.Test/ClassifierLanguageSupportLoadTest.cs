using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LanguageIdentification.Test
{
    [TestClass]
    public class ClassifierLanguageSupportLoadTest
    {
        #region Public 方法

        [TestMethod]
        public void ShouldDetectionFailWithOutLanguageLoaded()
        {
            var allSupportedLanguages = LanguageIdentificationClassifier.GetAllSupportedLanguages();

            var defaultClassifier = new LanguageIdentificationClassifier();

            foreach (var item in TestData.Items)
            {
                defaultClassifier.Reset();

                defaultClassifier.Append(item.Text);
                var defaultClassifierResult = defaultClassifier.Classify();

                Assert.AreEqual(item.LanguageCode, defaultClassifierResult.LanguageCode);

                var portionClassifier = new LanguageIdentificationClassifier(allSupportedLanguages.Where(m => m != defaultClassifierResult.LanguageCode));

                portionClassifier.Append(item.Text);
                var portionClassifierResult = portionClassifier.Classify();

                Assert.AreNotEqual(defaultClassifierResult.LanguageCode, portionClassifierResult.LanguageCode);

                Console.WriteLine($"default: {defaultClassifierResult} , portion: {portionClassifierResult}");
            }
        }

        [TestMethod]
        public void ShouldOnlyLoadedLanguageReturn()
        {
            var classifier = new LanguageIdentificationClassifier("zh", "en");

            foreach (var item in TestData.Items)
            {
                classifier.Reset();
                classifier.Append(item.Text);
                var result = classifier.Classify();

                Assert.IsTrue(result.LanguageCode == "zh" || result.LanguageCode == "en");

                if (item.LanguageCode == "zh" || item.LanguageCode == "en")
                {
                    Assert.AreEqual(item.LanguageCode, result.LanguageCode);
                }
            }
        }

        #endregion Public 方法
    }
}
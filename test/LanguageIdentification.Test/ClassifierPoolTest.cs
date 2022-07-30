using System;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LanguageIdentification.Test;

[TestClass]
public class LanguageIdentificationClassifierPoolTest
{
    #region Public 方法

    [TestMethod]
    public void ShouldBeSuccess()
    {
        foreach (var item in TestData.Items)
        {
            using var result = LanguageIdentificationClassifier.Classify(item.Text);
            Assert.AreEqual(item.LanguageCode, result.LanguageCode);

            Console.WriteLine(result);
        }
    }

    [TestMethod]
    public void ShouldBeSuccessInParallelRun()
    {
        Parallel.For(0, Environment.ProcessorCount * 1_000, _ =>
        {
            ShouldBeSuccess();
        });
    }

    #endregion Public 方法
}
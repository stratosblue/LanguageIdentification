using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LanguageIdentification.Test
{
    [TestClass]
    public class ModelSerializeTest
    {
        #region Public 方法

        [TestMethod]
        public void ShouldRestoreSameObject()
        {
            using var memoryStream = new MemoryStream(1024 * 1024 * 10);

            var model = LanguageIdentificationModel.Default;

            model.Serialize(memoryStream);

            memoryStream.Seek(0, SeekOrigin.Begin);

            var newModel = LanguageIdentificationModel.Deserialize(memoryStream);

            Assert.IsTrue(model.Equals(newModel));
        }

        #endregion Public 方法
    }
}
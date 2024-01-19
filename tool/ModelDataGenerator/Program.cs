using System;
using System.IO;
using System.IO.Compression;

using LanguageIdentification;

#pragma warning disable CS8602 // 解引用可能出现空引用。

namespace ModelDataGenerator;

internal class Program
{
    #region Private 方法

    private static void Main(string[] args)
    {
        using var sourceZipStream = File.OpenRead("langid.zip");
        using var zipArchive = new ZipArchive(sourceZipStream, ZipArchiveMode.Read);
        var zipArchiveEntry = zipArchive.GetEntry("langid.json");
        using var sourceStream = zipArchiveEntry.Open();
        using var sourceReader = new StreamReader(sourceStream);

        var sourceJson = sourceReader.ReadToEnd();
        var sourceModel = Newtonsoft.Json.JsonConvert.DeserializeObject<Model>(sourceJson);

        using var memoryStream = new MemoryStream();

        sourceModel.Serialize(memoryStream);

        memoryStream.Seek(0, SeekOrigin.Begin);

        using var targetStream = File.OpenWrite("langid-model-data");
        using var targetGzipStream = new GZipStream(targetStream, CompressionLevel.Optimal);

        memoryStream.CopyTo(targetGzipStream);

        memoryStream.Seek(0, SeekOrigin.Begin);

        var newModel = Model.Deserialize(memoryStream);

        var text = Newtonsoft.Json.JsonConvert.SerializeObject(sourceModel);
        var newText = Newtonsoft.Json.JsonConvert.SerializeObject(newModel);

        if (!string.Equals(text, newText, StringComparison.Ordinal))
        {
            throw new Exception("SerializeObject not equals SourceObject generate fail.");
        }

        Console.WriteLine("Generation complete");
    }

    #endregion Private 方法
}

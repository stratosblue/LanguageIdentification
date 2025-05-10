using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using LanguageIdentification;

#pragma warning disable CS8602 // 解引用可能出现空引用。

namespace ModelDataGenerator;

internal class Program
{
    #region Private 方法

    private static void Main(string[] args)
    {
        Console.WriteLine("Start Generate langid-model-data");

        var stopwatch = Stopwatch.StartNew();

        using var sourceZipStream = File.OpenRead(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "langid.zip"));
        using var zipArchive = new ZipArchive(sourceZipStream, ZipArchiveMode.Read);
        var zipArchiveEntry = zipArchive.GetEntry("langid.json");
        using var sourceStream = zipArchiveEntry.Open();
        using var sourceReader = new StreamReader(sourceStream);

        var sourceJson = sourceReader.ReadToEnd();
        var sourceModel = Newtonsoft.Json.JsonConvert.DeserializeObject<Model>(sourceJson);

        using var memoryStream = new MemoryStream();

        sourceModel.Serialize(memoryStream);

        memoryStream.Seek(0, SeekOrigin.Begin);

        var outputDir = Environment.GetEnvironmentVariable("OUTPUT_DIR");
        if (!string.IsNullOrEmpty(outputDir))
        {
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }
        }

        using var targetStream = File.OpenWrite(Path.Combine(outputDir ?? string.Empty, "langid-model-data"));
        targetStream.SetLength(0);
        using var targetGzipStream = new GZipStream(targetStream, CompressionLevel.SmallestSize);

        memoryStream.CopyTo(targetGzipStream);

        memoryStream.Seek(0, SeekOrigin.Begin);

        var newModel = Model.Deserialize(memoryStream);

        var text = Newtonsoft.Json.JsonConvert.SerializeObject(sourceModel);
        var newText = Newtonsoft.Json.JsonConvert.SerializeObject(newModel);

        if (!string.Equals(text, newText, StringComparison.Ordinal))
        {
            throw new Exception("SerializeObject not equals SourceObject generate fail.");
        }

        stopwatch.Stop();

        Console.WriteLine($"langid-model-data Generation complete. During {stopwatch.Elapsed}");
    }

    #endregion Private 方法
}

using System;
using System.Diagnostics;

namespace LanguageIdentification.TestConsole;

class Program
{
    public const string Text_ES = @"Tabla de contenidos
Parte I: Información general ...................................................................................................................3
Parte II: Texto completo del anuncio.........................................................................................................4
Sec.I: DESCRIPCIÓN DE LA OPORTUNIDAD DE FINANCIACIÓN........................................4
A. Objetivos del programa ..................................................................................................................7
B. Estructura del programa..................................................................................................................9
C. Áreas Técnicas..................................................................................................................10
Sec II. INFORMACIÓN SOBRE EL PREMIO ......................................................................25
A. Investigación Fundamental............................................................................................................26
Sec.III: INFORMACIÓN DE ELEGIBILIDAD....................................................................27
A. Solicitantes elegibles. .........................................................................................................28
B. Integridad de las adquisiciones, normas de conducta, consideraciones éticas y conflictos de intereses
organizacionales .................................................................................................................................28
C. Compartimiento/Costes ................................................................................................................29
Sec.IV: INFORMACIÓN DE SOLICITUD Y PRESENTACIÓN..............................................29";

    static void Main(string[] args)
    {
        var stopwatch = Stopwatch.StartNew();

        var langIdClassifier = new LanguageIdentificationClassifier();

        stopwatch.Stop();
        Console.WriteLine($"Load Time: {stopwatch.ElapsedMilliseconds}");

        langIdClassifier.Append(Text_ES);
        using var result = langIdClassifier.Classify();

        Console.WriteLine(result);
        Console.WriteLine("---- Rank ----");
        using var detectedLanguages = langIdClassifier.CreateRank();
        foreach (var item in detectedLanguages)
        {
            Console.WriteLine(item);
        }
        Console.ReadLine();
    }
}

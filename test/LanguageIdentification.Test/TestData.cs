using System.Collections.Generic;

namespace LanguageIdentification.Test;

internal static class TestData
{
    #region Public 属性

    public static IReadOnlyList<TestDataItem> Items { get; }

    #endregion Public 属性

    #region Public 构造函数

    static TestData()
    {
        var items = new List<TestDataItem>
        {
            new("es", @"Tabla de contenidos
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
B. Integridad de las adquisiciones, normas de conducta, consideraciones éticas y conflictos de intereses"),

            new("zh", "好的"),
            new("en", "hello"),
            new("ja", "使用マニュアル"),
            new("ko", "안녕하세요"),
            new("ru", "Здравствыйте")
        };

        Items = items;
    }

    #endregion Public 构造函数
}

internal class TestDataItem
{
    #region Public 属性

    public string LanguageCode { get; }

    public string Text { get; }

    #endregion Public 属性

    #region Public 构造函数

    public TestDataItem(string languageCode, string text)
    {
        if (string.IsNullOrEmpty(languageCode))
        {
            throw new System.ArgumentException($"“{nameof(languageCode)}”不能为 null 或空。", nameof(languageCode));
        }

        if (string.IsNullOrEmpty(text))
        {
            throw new System.ArgumentException($"“{nameof(text)}”不能为 null 或空。", nameof(text));
        }

        LanguageCode = languageCode;
        Text = text;
    }

    #endregion Public 构造函数
}

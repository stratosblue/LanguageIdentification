# LanguageIdentification

## Intro

.NET Port of Language Identification Library for [langid-java](https://github.com/carrotsearch/langid-java)。

移植自[langid-java](https://github.com/carrotsearch/langid-java)的语言识别库，技术细节参见[langid-java](https://github.com/carrotsearch/langid-java)、[langid.py](https://github.com/saffsd/langid.py)。

 - 支持`.netstandard2.0`+；

## 如何使用

### 安装Nuget包

```PowerShell
Install-Package LanguageIdentification
```

### 快速使用

----

1. 通过手动创建实例使用
```C#
var langIdClassifier = new LanguageIdentificationClassifier();
langIdClassifier.Append("Hello");
var result = langIdClassifier.Classify();
Console.WriteLine(result);
```

 - 实例`不是线程安全`的；
 - 实例复用进行新的检测前，需要调用`Reset()`方法；

----

2. 通过静态方法使用
```C#
var result = LanguageIdentificationClassifier.Classify("Hello");
Console.WriteLine(result);
```

 - 静态方法是`线程安全`的，内部使用了默认的`LanguageIdentificationClassifier`池 - `LanguageIdentificationClassifierPool.Default` 进行处理；


### 特殊用法

----

1. 只加载部分语言支持
```C#
var classifier = new LanguageIdentificationClassifier(new[] { "zh", "en" });
langIdClassifier.Append("Hello");
var result = langIdClassifier.Classify();
Console.WriteLine(result);
```
 - 速度会更快；
 - 返回的语言只会是已加载语言的其中一个；

----

2. 使用自己的模型数据
```C#
var model = new LanguageIdentificationModel(langClasses, nb_ptc, nb_pc, dsa, dsaOutput);
var classifier = new LanguageIdentificationClassifier(model);
```

 - 具体各个参数是什么意义。。不清楚。。自行研究源项目。。。

----

3. 加载默认模型时，使用更少的内存

```C#
AppContext.SetSwitch("LanguageIdentification:TryLoadModelWithLowMemory", true);
```

 - 在使用前设置开关；
 - 加载时会使用更少的内存，对应的会增加加载时间；
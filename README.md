# Lucene.Net.Analysis.MMSeg #
基于Chih-Hao Tsai 的 MMSeg 算法(http://technology.chtsai.org/mmseg/ )实现的中文分词器，并实现 lucene.net 的 analyzer以方便在Lucene.Net中使用。本代码来源于*王员外*(http://www.cnblogs.com/land/archive/2011/07/19/mmseg4j.html )基于Java版的翻译，升级到了最新版Lucene.Net (≥ 3.0.3)，并包含简单示例和NuGet安装包。


# NuGet地址 #
https://nuget.org/packages/Lucene.Net.Analysis.MMSeg/


# 使用 #
一共三种搜索模式供选择：

SimpleAnalyzer
```csharp
Analyzer analyzer = new SimpleAnalyzer();
```

MaxWordAnalyzer
```csharp
Analyzer analyzer = new MaxWordAnalyzer();
```

ComplexAnalyzer
```csharp
Analyzer analyzer = new ComplexAnalyzer();
```

具体使用方法，请参考代码中的示例和lucene.net的文档

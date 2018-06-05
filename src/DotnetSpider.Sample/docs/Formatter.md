#### Formatter

用于对数据的格式化, 比如 unix 时间转成 DateTime. 默认已经实现了不少 Formatter:

* CharacterCaseFormater
* CutoutFormatter
* DigitUnitFormater
* DisplaceFormater
* Download
* HtmlDecodeFormater
* RegexAppendFormatter
* RegexFormater
* RegexReplaceFormater
* ReplaceFormater
* SplitFormater
* SplitToListFormater
* StringFormater
* TimeStampFormater
* TrimFormater
* UrlEncodeFormater

#### 自定义 Formatter

继承 DotnetSpider.Extension.Model.Formatter.Formatter

参考例子： DotnetSpider.Sample.docs.CustomizeFormatterSpider
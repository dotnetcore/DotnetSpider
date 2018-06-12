using DotnetSpider.Core;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Model.Formatter;
using DotnetSpider.Extension.Pipeline;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace DotnetSpider.Sample.docs
{
	/// <summary>
	/// After spider complete, check the data in mysql, website are null in every row.
	/// </summary>
	public class CustomizeFormatter
	{
		public static void Run()
		{
			BaiduSearchSpider spider = new BaiduSearchSpider();
			spider.Run();
		}
	}

	public class NullFormatter : Formatter
	{
		protected override void CheckArguments()
		{
		}

		protected override object FormateValue(object value)
		{
			return "";
		}
	}

	[TaskName("baidu_search")]
	public class BaiduSearchSpider : EntitySpider
	{
		public BaiduSearchSpider() : base("EntityModelSpider")
		{
			EmptySleepTime = 1000;
		}

		protected override void MyInit(params string[] arguments)
		{
			var word = "可乐|雪碧";
			AddStartUrl(string.Format("http://news.baidu.com/ns?word={0}&tn=news&from=news&cl=2&pn=0&rn=20&ct=1", word), new Dictionary<string, dynamic> { { "Keyword", word } });
			AddEntityType<BaiduSearchEntry>();
			AddPipeline(new MySqlEntityPipeline("Database='mysql';Data Source=localhost;User ID=root;Port=3306;SslMode=None;"));
		}

		[TableInfo("baidu", "baidu_search_customizeforamtter")]
		[EntitySelector(Expression = ".//div[@class='result']", Type = SelectorType.XPath)]
		class BaiduSearchEntry : BaseEntity
		{
			[Field(Expression = "Keyword", Type = SelectorType.Enviroment)]
			public string Keyword { get; set; }

			[Field(Expression = ".//h3[@class='c-title']/a")]
			[ReplaceFormatter(NewValue = "", OldValue = "<em>")]
			[ReplaceFormatter(NewValue = "", OldValue = "</em>")]
			public string Title { get; set; }

			[Field(Expression = ".//h3[@class='c-title']/a/@href")]
			public string Url { get; set; }

			[Field(Expression = ".//div/p[@class='c-author']/text()")]
			[NullFormatter]
			public string Website { get; set; }
		}
	}
}

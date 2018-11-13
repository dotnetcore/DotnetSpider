using DotnetSpider.Core;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Processor;
using DotnetSpider.Extraction;
using DotnetSpider.Extraction.Model.Attribute;

namespace DotnetSpider.Sample.docs
{
	class OrmSpider
	{
		public static void Run()
		{
			var entityProcessor = new EntityProcessor<BaiduSearchEntry>();
			var spider = Spider.Create();
		}

		[Schema("baidu", "baidu_search_entity_model")]
		class BaiduSearchEntry : BaseEntity
		{
			[Column]
			[Field(Expression = "", Type = SelectorType.Enviroment)]
			public string Keyword { get; set; }

			[Column]
			[Field(Expression = "", Type = SelectorType.Enviroment)]
			public string Title { get; set; }

			[Column]
			[Field(Expression = "", Type = SelectorType.Enviroment)]
			public string Url { get; set; }

			[Column]
			[Field(Expression = "", Type = SelectorType.Enviroment)]
			public string Website { get; set; }

			[Column]
			[Field(Expression = "", Type = SelectorType.Enviroment)]
			public string Snapshot { get; set; }

			[Column]
			[Field(Expression = "", Type = SelectorType.Enviroment)]
			public string Details { get; set; }

			[Column(Length = 0)]
			[Field(Expression = "", Type = SelectorType.Enviroment)]
			public string PlainText { get; set; }
		}
	}
}

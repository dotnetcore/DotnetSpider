using DotnetSpider.Core;
using DotnetSpider.Extension.Processor;
using DotnetSpider.Extraction;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Extraction.Model.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Sample.docs
{
	class OrmSpider
	{
		public static void Run()
		{
			var entityProcessor = new EntityProcessor<BaiduSearchEntry>();
			var spider = Spider.Create();
		}

		[TableInfo("baidu", "baidu_search_entity_model")]
		class BaiduSearchEntry : BaseEntity
		{
			[FieldSelector(Expression = "", Type = SelectorType.Enviroment)]
			public string Keyword { get; set; }

			[FieldSelector(Expression = "", Type = SelectorType.Enviroment)]
			public string Title { get; set; }

			[FieldSelector(Expression = "", Type = SelectorType.Enviroment)]
			public string Url { get; set; }

			[FieldSelector(Expression = "", Type = SelectorType.Enviroment)]
			public string Website { get; set; }

			[FieldSelector(Expression = "", Type = SelectorType.Enviroment)]
			public string Snapshot { get; set; }

			[FieldSelector(Expression = "", Type = SelectorType.Enviroment)]
			public string Details { get; set; }

			[FieldSelector(Expression = "", Type = SelectorType.Enviroment)]
			public string PlainText { get; set; }
		}
	}
}

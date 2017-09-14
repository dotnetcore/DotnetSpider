using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Model.Attribute;
using System;

namespace DotnetSpider.Extension.Model
{
	[EntityTable("crawler_cache", "cache", EntityTable.Today, Indexs = new[] { "Url", "Identity", "TaskId", "Name" })]
	internal class Cache : SpiderEntity
	{
		[PropertyDefine(Expression = "", Type = SelectorType.Enviroment, Length = 120)]
		public string Identity { get; set; }

		[PropertyDefine(Expression = "", Type = SelectorType.Enviroment, Length = 120)]
		public string TaskId { get; set; }

		[PropertyDefine(Expression = "", Type = SelectorType.Enviroment, Length = 120)]
		public string Name { get; set; }

		[PropertyDefine(Expression = "", Type = SelectorType.Enviroment, Length = 255)]
		public string Url { get; set; }

		[PropertyDefine(Expression = "", Type = SelectorType.Enviroment)]
		public string Content { get; set; }

		[PropertyDefine(Expression = "", Type = SelectorType.Enviroment)]
		public DateTime CDate { get; set; }
	}
}

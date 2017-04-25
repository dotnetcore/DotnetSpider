using System;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Core;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Pipeline;

namespace DotnetSpider.Sample
{
	public class RegexTestEntitySpider : EntitySpiderBuilder
	{
		protected override EntitySpider GetEntitySpider()
		{
			EntitySpider context = new EntitySpider(new Site());
			context.SetTaskGroup("cnblogs homepage");
			context.SetIdentity("cnblogs homepage " + DateTime.Now.ToString("yyyy-MM-dd HHmmss"));
			context.AddStartUrl("http://www.cnblogs.com");
			context.AddPipeline(new ConsoleEntityPipeline());
			context.AddEntityType(typeof(HomePage));
			return context;
		}

		public class HomePage : ISpiderEntity
		{
			//jQuery(".yk-rank div:1")
			[PropertyDefine(Expression = "<a.*?т╟вс</a>", Type = SelectorType.Regex, Argument = "1")]
			public string Category { get; set; }
		}
	}
}
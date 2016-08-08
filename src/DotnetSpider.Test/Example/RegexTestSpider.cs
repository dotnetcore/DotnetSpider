using System;
using DotnetSpider.Extension;
using DotnetSpider.Core;
using System.Collections.Generic;
using DotnetSpider.Extension.Configuration;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;

namespace DotnetSpider.Test.Example
{
	public class RegexTestSpider : SpiderBuilder
	{
		protected override SpiderContext GetSpiderContext()
		{
			SpiderContext context = new SpiderContext();
			context.SetTaskGroup("cnblogs homepage");
			context.SetSpiderName("cnblogs homepage " + DateTime.Now.ToString("yyyy-MM-dd HHmmss"));
			context.AddStartUrl("http://www.cnblogs.com");
			context.AddPipeline(new ConslePipeline());
			context.AddEntityType(typeof(HomePage));
			return context;
		}

		public class HomePage : ISpiderEntity
		{
			//jQuery(".yk-rank div:1")
			[PropertyExtractBy(Expression = "<a.*?т╟вс</a>", Type = ExtractType.Regex, Argument = 1)]
			public string Category { get; set; }
		}
	}
}
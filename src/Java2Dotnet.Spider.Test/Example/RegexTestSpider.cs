using System;
using Java2Dotnet.Spider.Extension;
using Java2Dotnet.Spider.Core;
using System.Collections.Generic;
using Java2Dotnet.Spider.Extension.Configuration;
using Java2Dotnet.Spider.Extension.Model;
using Java2Dotnet.Spider.Extension.Model.Attribute;

namespace Java2Dotnet.Spider.Test.Example
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
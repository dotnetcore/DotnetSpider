using System;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Configuration;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Core;

namespace DotnetSpider.Sample
{
	public class RegexTestSpider : SpiderBuilder
	{
		protected override EntitySpider GetSpiderContext()
		{
			EntitySpider context = new EntitySpider(new Site());
			context.SetTaskGroup("cnblogs homepage");
			context.SetIdentity("cnblogs homepage " + DateTime.Now.ToString("yyyy-MM-dd HHmmss"));
			context.AddStartUrl("http://www.cnblogs.com");
			context.AddEntityPipeline(new ConslePipeline());
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
using System;
using Java2Dotnet.Spider.Extension;
using Java2Dotnet.Spider.Core;
using System.Collections.Generic;
using Java2Dotnet.Spider.Extension.Configuration;
using Java2Dotnet.Spider.Extension.Model;
using Java2Dotnet.Spider.Extension.Model.Attribute;

namespace Java2Dotnet.Spider.Test.Example
{
	public class RegexTestSpider : ISpiderContext
	{
		public SpiderContextBuilder GetBuilder()
		{
			return new SpiderContextBuilder(new SpiderContext
			{
				UserId = "dotnetspider",
				TaskGroup = "cnblogs homepage",
				SpiderName = "cnblogs homepage " + DateTime.Now.ToString("yyyy-MM-dd HHmmss"),
				ThreadNum = 1,
				Site = new Site
				{
				},
				StartUrls = new Dictionary<string, Dictionary<string, object>>
				{
					{"http://www.cnblogs.com", null},
				},

				Scheduler = new QueueScheduler(),
				Pipeline = new ConslePipeline(),
			}, typeof(HomePage));
		}

		public class HomePage : ISpiderEntity
		{
			[PropertyExtractBy(Expression = "<a.*?т╟вс</a>", Type = ExtractType.Regex, Argument = 1)]
			public string Category { get; set; }
		}
	}
}
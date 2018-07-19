using DotnetSpider.Common;
using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Infrastructure.Database;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Processor.TargetRequestExtractors;
using DotnetSpider.Core.Scheduler;
using DotnetSpider.Downloader;
using DotnetSpider.Downloader.AfterDownloadCompleteHandlers;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extension.Processor;
using DotnetSpider.Extraction;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Extraction.Model.Attribute;
using DotnetSpider.Extraction.Model.Formatter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DotnetSpider.Sample.docs
{
	public class RegexSelectorSpider : EntitySpider
	{
		protected override void MyInit(params string[] arguments)
		{
			AddStartUrl("http://www.cnblogs.com");
			AddPipeline(new ConsoleEntityPipeline());
			AddEntityType<HomePage>();
		}

		class HomePage
		{
			[Field(Expression = "<a.*?т╟вс</a>", Type = SelectorType.Regex, Arguments = "1")]
			public string Category { get; set; }
		}
	}
}
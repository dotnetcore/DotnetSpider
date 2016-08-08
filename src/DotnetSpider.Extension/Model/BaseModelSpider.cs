using System;
using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Scheduler;
using DotnetSpider.Extension.Processor;

namespace DotnetSpider.Extension.Model
{
	public class BaseModelSpider : Core.Spider
	{
		public const string PipelineModel = "PipelineModel";

		protected BaseModelSpider(Site site, string identify, string userid, string taskGroup, IPageProcessor pageProcessor, IScheduler scheduler) : base(site, identify, userid, taskGroup, pageProcessor, scheduler)
		{
		}

		public void SetCustomizeTargetUrls(Func<Page, IList<Request>> getCustomizeTargetUrls)
		{
			var processor = PageProcessor as EntityProcessor;
			if (processor != null)
			{
				processor.GetCustomizeTargetUrls = getCustomizeTargetUrls;
			}
		}

		public void SetCachedSize(int count)
		{
			foreach (var pipeline in Pipelines)
			{
				var cachedPipeline = pipeline as CachedPipeline;
				if (cachedPipeline != null)
				{
					cachedPipeline.CachedSize = count;
				}
			}
		}
	}
}


using Java2Dotnet.Spider.Extension.Configuration;
using Java2Dotnet.Spider.Extension.Configuration.Json;
using Newtonsoft.Json;
using System;

namespace Java2Dotnet.Spider.Extension
{
	public abstract class SpiderBuilder
	{
		protected virtual Action AfterSpiderFinished { get; set; }

		protected abstract SpiderContext GetSpiderContext();

		public void Run(params string[] args)
		{
			var context = GetSpiderContext();
			if (context.Scheduler == null)
			{
				context.Scheduler = new QueueScheduler();
			}
#if Test
			// 转成JSON再转换成SpiderContext, 用于测试JsonSpiderContext是否正常
			string json = JsonConvert.SerializeObject(GetSpiderContext());
			ModelSpider spider = new ModelSpider(JsonConvert.DeserializeObject<JsonSpiderContext>(json).ToRuntimeContext());
#elif Publish
			ModelSpider spider = new ModelSpider(context);
#endif
			spider.AfterSpiderFinished = AfterSpiderFinished;
			spider.Run(args);
		}
	}
}
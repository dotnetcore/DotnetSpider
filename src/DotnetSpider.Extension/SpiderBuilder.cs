using DotnetSpider.Extension.Configuration;
using System;

namespace DotnetSpider.Extension
{
	public abstract class SpiderBuilder
	{
		protected abstract EntitySpider GetSpiderContext();

		public void Run(params string[] args)
		{
			var spider = GetSpiderContext();
			if (spider != null)
			{
#if Test
	// 转成JSON再转换成SpiderContext, 用于测试JsonSpiderContext是否正常
			string json = JsonConvert.SerializeObject(GetSpiderContext());
			ModelSpider spider = new ModelSpider(JsonConvert.DeserializeObject<JsonSpiderContext>(json).ToRuntimeContext());
#elif Publish
				//ModelSpider spider = new ModelSpider(context) {AfterSpiderFinished = AfterSpiderFinished};
#endif
				spider.Run(args);
			}
		}
	}
}
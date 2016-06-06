
using Java2Dotnet.Spider.Extension.Configuration.Json;
using Newtonsoft.Json;

namespace Java2Dotnet.Spider.Extension
{
	public abstract class SpiderBuilder
	{
		protected abstract SpiderContext GetSpiderContext();

		public void Run(params string[] args)
		{
#if Test
			// 转成JSON再转换成SpiderContext, 用于测试JsonSpiderContext是否正常
			string json = JsonConvert.SerializeObject(GetSpiderContext());
			ModelSpider spider = new ModelSpider(JsonConvert.DeserializeObject<JsonSpiderContext>(json).ToRuntimeContext());
			spider.Run(args);
#elif Publish
			ModelSpider spider = new ModelSpider(GetSpiderContext());
			spider.Run(args);
#endif
		}
	}
}
#if Test
using Newtonsoft.Json;
#endif

namespace DotnetSpider.Extension
{
	public abstract class EntitySpiderBuilder
	{
		protected abstract EntitySpider GetEntitySpider();

		public virtual void Run(params string[] args)
		{
			var spider = GetEntitySpider();
#if Test
	// 转成JSON再转换成SpiderContext, 用于测试JsonSpiderContext是否正常
			string json = JsonConvert.SerializeObject(GetSpiderContext());
			ModelSpider spider = new ModelSpider(JsonConvert.DeserializeObject<JsonSpiderContext>(json).ToRuntimeContext());
#elif Publish
			//ModelSpider spider = new ModelSpider(context) {AfterSpiderFinished = AfterSpiderFinished};
#endif
			spider?.Run(args);
		}
	}
}
using Newtonsoft.Json;

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
	// ת��JSON��ת����SpiderContext, ���ڲ���JsonSpiderContext�Ƿ�����
			string json = JsonConvert.SerializeObject(GetSpiderContext());
			ModelSpider spider = new ModelSpider(JsonConvert.DeserializeObject<JsonSpiderContext>(json).ToRuntimeContext());
#elif Publish
				//ModelSpider spider = new ModelSpider(context) {AfterSpiderFinished = AfterSpiderFinished};
#endif
				//string json = JsonConvert.SerializeObject(GetSpiderContext());
				spider.Run(args);
			}
		}
	}
}
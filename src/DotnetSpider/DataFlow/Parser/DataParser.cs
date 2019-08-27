using System.Threading.Tasks;
using DotnetSpider.Downloader;

namespace DotnetSpider.DataFlow.Parser
{
	/// <summary>
	/// 默认数据解析器
	/// </summary>
	public class DataParser : DataParserBase
	{
		protected override Task<DataFlowResult> Parse(DataFlowContext context)
		{
			if (context.Response != null)
			{
				context.AddData("URL", context.Response.Request.Url);
				context.AddData("Content", context.Response.GetRawtext());
				context.AddData("TargetUrl", context.Response.TargetUrl);
				context.AddData("Success", context.Response.Success);
				context.AddData("ElapsedMilliseconds", context.Response.ElapsedMilliseconds);
			}

			return Task.FromResult(DataFlowResult.Success);
		}
	}
}

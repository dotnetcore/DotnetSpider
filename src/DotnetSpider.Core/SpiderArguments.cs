namespace DotnetSpider.Core
{
	public static class SpiderArguments
	{
		/// <summary>
		/// 只运行到InitComponent, 不真正的运行爬虫, 用于测试加载
		/// </summary>
		public const string InitOnly = "initonly";

		/// <summary>
		/// 在往Scheduler中添加Request前, 清空当前Scheduler
		/// </summary>
		public const string Reset = "reset";

		/// <summary>
		/// 不执行StartRequestsBuilder
		/// </summary>
		public const string ExcludeRequestBuilder = "excludebuilder";

		/// <summary>
		/// 只执行报告操作
		/// </summary>
		public const string Report = "report";
	}
}

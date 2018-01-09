using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;

namespace DotnetSpider.Extension.Downloader
{
	/// <summary>
	/// 下载内容缓存对象
	/// </summary>
	[EntityTable("crawl_cache", "cache", EntityTable.Today, Indexs = new[] { "Url", "Identity", "TaskId", "Name" })]
	public class DownloadCacheData : SpiderEntity
	{
		/// <summary>
		/// 所属爬虫的唯一标识
		/// </summary>
		[PropertyDefine(Expression = "", Type = SelectorType.Enviroment, Length = 120)]
		public string Identity { get; set; }

		/// <summary>
		/// 所属爬虫的任务编号
		/// </summary>
		[PropertyDefine(Expression = "", Type = SelectorType.Enviroment, Length = 120)]
		public string TaskId { get; set; }

		/// <summary>
		/// 所属爬虫的名称
		/// </summary>
		[PropertyDefine(Expression = "", Type = SelectorType.Enviroment, Length = 120)]
		public string Name { get; set; }

		/// <summary>
		/// 采集的链接
		/// </summary>
		[PropertyDefine(Expression = "", Type = SelectorType.Enviroment, Length = 255)]
		public string Url { get; set; }

		/// <summary>
		/// 下载的内容
		/// </summary>
		[PropertyDefine(Expression = "", Type = SelectorType.Enviroment)]
		public string Content { get; set; }
	}
}

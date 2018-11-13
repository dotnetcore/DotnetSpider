using DotnetSpider.Extension.Model;
using DotnetSpider.Extraction;
using DotnetSpider.Extraction.Model.Attribute;

namespace DotnetSpider.Extension.Downloader
{
	/// <summary>
	/// 下载内容缓存对象
	/// </summary>
	[Schema("crawl_cache", "cache", TableNamePostfix.Today)]
	public class DownloadCache : BaseEntity
	{
		/// <summary>
		/// 所属爬虫的唯一标识
		/// </summary>
		[Field(Expression = "", Type = SelectorType.Enviroment)]
		[Column]
		[Index("URL_IDENTITY_TASK_ID_NAME")]
		public string Identity { get; set; }

		/// <summary>
		/// 所属爬虫的任务编号
		/// </summary>
		[Field(Expression = "", Type = SelectorType.Enviroment)]
		[Column]
		[Index("URL_IDENTITY_TASK_ID_NAME")]
		public string TaskId { get; set; }

		/// <summary>
		/// 所属爬虫的名称
		/// </summary>
		[Field(Expression = "", Type = SelectorType.Enviroment)]
		[Column]
		[Index("URL_IDENTITY_TASK_ID_NAME")]
		public string Name { get; set; }

		/// <summary>
		/// 采集的链接
		/// </summary>
		[Field(Expression = "", Type = SelectorType.Enviroment)]
		[Column]
		[Index("URL_IDENTITY_TASK_ID_NAME")]
		public string Url { get; set; }

		/// <summary>
		/// 下载的内容
		/// </summary>
		[Field(Expression = "", Type = SelectorType.Enviroment)]
		[Column]
		public string Content { get; set; }
	}
}

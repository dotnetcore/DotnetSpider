using System.Collections;
using System.Collections.Generic;

namespace DotnetSpider.Core.Processor
{
	/// <summary>
	/// 默认解析器, 没有特别大的作用, 用于测试等
	/// </summary>
	public class DefaultPageProcessor : BasePageProcessor
	{
		/// <summary>
		/// 解析页面数据
		/// </summary>
		/// <param name="page">页面数据</param>
		protected override void Handle(Page page)
		{
			page.AddResultItem("title", page.Selectable().XPath("//title").GetValue());
			page.AddResultItem("html", page.Content);
			page.AddResultItem("url", page.Request.Url);
		}
	}

	class Item
	{
		public string Title { get; set; }
	}
}

using System.Text;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace DotnetSpider.Portal
{/// <summary>
 /// 分页option属性
 /// </summary>
	public class MoPagerOption
	{
		/// <summary>
		/// 当前页 必传
		/// </summary>
		public int CurrentPage { get; set; }
		/// <summary>
		/// 总条数 必传
		/// </summary>
		public long Total { get; set; }

		/// <summary>
		/// 分页记录数（每页条数 默认每页15条）
		/// </summary>
		public int PageSize { get; set; }

		/// <summary>
		/// 路由地址(格式如：/Controller/Action) 默认自动获取
		/// </summary>
		public string RouteUrl { get; set; }

		/// <summary>
		/// 样式 默认 bootstrap样式 1
		/// </summary>
		public int StyleNum { get; set; }
	}

	public class PagerTagHelper : TagHelper
	{
		public MoPagerOption PagerOption { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			output.TagName = "div";

			if (PagerOption.PageSize <= 0) { PagerOption.PageSize = 15; }
			if (PagerOption.CurrentPage <= 0) { PagerOption.CurrentPage = 1; }

			if (PagerOption.Total <= 0) { return; }

			//总页数
			var totalPage = PagerOption.Total / PagerOption.PageSize + (PagerOption.Total % PagerOption.PageSize > 0 ? 1 : 0);
			if (totalPage <= 0) { return; }
			if (PagerOption.CurrentPage > totalPage)
			{
				PagerOption.CurrentPage = (int)totalPage;
			}
			//当前路由地址
			if (string.IsNullOrEmpty(PagerOption.RouteUrl))
			{
				//PagerOption.RouteUrl = helper.ViewContext.HttpContext.Request.RawUrl;
				if (!string.IsNullOrEmpty(PagerOption.RouteUrl))
				{

					var lastIndex = PagerOption.RouteUrl.LastIndexOf("/");
					PagerOption.RouteUrl = PagerOption.RouteUrl.Substring(0, lastIndex);
				}
			}
			PagerOption.RouteUrl = PagerOption.RouteUrl.TrimEnd('/');

			//构造分页样式
			var sbPage = new StringBuilder(string.Empty);
			switch (PagerOption.StyleNum)
			{
				case 2:
					{
						break;
					}
				default:
					{
						#region 默认样式

						sbPage.Append("<nav>");
						sbPage.Append(" <ul class=\"pagination\">");
						if (PagerOption.CurrentPage > 1)
						{
							sbPage.AppendFormat("<li><a href=\"{0}/{1}\" aria-label=\"Previous\"><span aria-hidden=\"true\">Previous</span></a></li>",
								PagerOption.RouteUrl,
								PagerOption.CurrentPage - 1 <= 0 ? 1 : PagerOption.CurrentPage - 1);
						}
 
						var spitindex = PagerOption.CurrentPage - 2;

						if (spitindex > 4)
						{
							sbPage.AppendFormat("<li><a href=\"{0}/{1}\">1</a></li>", PagerOption.RouteUrl,1);
							sbPage.AppendFormat("<li><a href=\"{0}/{1}\">...</a></li>", PagerOption.RouteUrl, spitindex - 1);
						}
						else
						{
							for (int i = 0; i < spitindex; i++)
							{
								sbPage.AppendFormat("<li><a href=\"{0}/{1}\">{1}</a></li>", PagerOption.RouteUrl, i + 1);
							}
						}

						for (int i = PagerOption.CurrentPage - 1; i < PagerOption.CurrentPage; i++)
						{
							if (i >= PagerOption.CurrentPage || i < 0)
							{
								continue;
							}
							sbPage.AppendFormat("<li><a href=\"{0}/{1}\">{1}</a></li>", PagerOption.RouteUrl, i + 1);
						}

						sbPage.AppendFormat("<li class='active'><a><b>{0}</b></a></li>", PagerOption.CurrentPage);
						for (int i = PagerOption.CurrentPage + 1; i < PagerOption.CurrentPage; i++)
						{
							if (i >= PagerOption.CurrentPage + 3)
							{
								break;
							}
							sbPage.AppendFormat("<li><a href=\"{0}/{1}\">{1}</a></li>", PagerOption.RouteUrl, i + 1);
						}
						spitindex = PagerOption.CurrentPage + 3;

						if (PagerOption.CurrentPage - 4 > spitindex)
						{
							sbPage.AppendFormat("<li><a href=\"{0}/{1}\">...</a></li>", PagerOption.RouteUrl, spitindex + 2);

							sbPage.AppendFormat("<li><a href=\"{0}/{1}\">{1}</a></li>", PagerOption.RouteUrl, totalPage);
						}
						else
						{
							for (int i = spitindex; i < totalPage; i++)
							{
								sbPage.AppendFormat("<li><a href=\"{0}/{1}\">{1}</a></li>", PagerOption.RouteUrl, i + 1);
							}
						}
						if (PagerOption.CurrentPage != totalPage - 1)
						{
							sbPage.Append("  <li>");
							sbPage.AppendFormat("   <a href=\"{0}/{1}\" aria-label=\"Next\">",
								PagerOption.RouteUrl,
								PagerOption.CurrentPage + 1 > totalPage ? PagerOption.CurrentPage : PagerOption.CurrentPage + 1);
							sbPage.Append("    <span aria-hidden=\"true\">Next</span>");
							sbPage.Append("   </a>");
							sbPage.Append("  </li>");
						}
						sbPage.AppendFormat($" <li><a href=\"javascript:\">Total: {PagerOption.Total}</a></li>");
						sbPage.Append(" </ul>");
						sbPage.Append("</nav>");
						#endregion
					}
					break;
			}

			output.Content.SetHtmlContent(sbPage.ToString());
			//output.TagMode = TagMode.SelfClosing;
			//return base.ProcessAsync(context, output);
		}

	}
}

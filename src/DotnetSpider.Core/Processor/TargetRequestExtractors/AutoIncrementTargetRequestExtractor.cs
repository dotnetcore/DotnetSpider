using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DotnetSpider.Common;

namespace DotnetSpider.Core.Processor.TargetRequestExtractors
{
	/// <summary>
	/// 通过自增计算出新的目标链接, 比如: www.a.com/1.html-&gt;www.a.com/2.html
	/// </summary>
	public class AutoIncrementTargetRequestExtractor : TargetRequestExtractor
	{
		private readonly int _interval;
		private readonly Regex _pattern;
		private readonly string _paginationStr;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="paginationStr">URL中分页的部分, 如: www.a.com/content_1.html, 则可以填此值为 content_1.html, tent_1.html等, 框架会把数据部分改成\d+用于正则匹配截取</param>
		/// <param name="interval">每次自增的间隔</param>
		/// <param name="termination">中止器, 用于判断是否已到最后一个需要采集的链接</param>
		public AutoIncrementTargetRequestExtractor(string paginationStr, int interval = 1,
			ITargetRequestExtractorTermination termination = null)
		{
			if (string.IsNullOrWhiteSpace(paginationStr))
			{
				throw new ArgumentException($"{nameof(Extraction)} is null/empty.");
			}
			_interval = interval;
			_paginationStr = paginationStr;
			_pattern = new Regex($"{RegexUtil.Number.Replace(_paginationStr, @"\d+")}");
			Termination = termination;
		}

		/// <summary>
		/// 解析出目标链接
		/// </summary>
		/// <param name="response">页面数据</param>
		/// <returns>目标链接</returns>
		protected override IEnumerable<Request> Extract(Response response)
		{
			var currentPageStr = GetCurrentPagination(response.Request.Url);
			var matches = RegexUtil.Number.Matches(currentPageStr);
			if (matches.Count > 0 && int.TryParse(matches[0].Value, out var currentPage))
			{
				var next = RegexUtil.Number.Replace(_paginationStr, (currentPage + _interval).ToString());
				string newUrl = response.Request.Url.Replace(currentPageStr, next);
				return new[] { new Request(newUrl, response.Request.Properties) { Site = response.Request.Site } };
			}

			return new Request[0];
		}

		/// <summary>
		/// 取得当前分页
		/// </summary>
		/// <param name="currentUrlOrContent">当前链接或者内容(有的分页信息放在Cookie或者Post的内容里)</param>
		/// <returns></returns>
		protected virtual string GetCurrentPagination(string currentUrlOrContent)
		{
			return _pattern.Match(currentUrlOrContent).Value;
		}
	}
}

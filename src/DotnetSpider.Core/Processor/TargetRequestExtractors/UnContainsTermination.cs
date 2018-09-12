using System.Linq;
using DotnetSpider.Common;
using DotnetSpider.Downloader;

namespace DotnetSpider.Core.Processor.TargetRequestExtractors
{
	/// <summary>
	/// 如果不包含指定内容则到了最后一个采集链接
	/// </summary>
	public class UnContainsTermination : ITargetRequestExtractorTermination
	{
		private readonly string[] _unContains;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="unContains">不包含的内容</param>
		public UnContainsTermination(string[] unContains)
		{
			_unContains = unContains;
		}

		/// <summary>
		/// 是否到了最后一个链接
		/// </summary>
		/// <param name="response">页面数据</param>
		/// <returns>如果返回 True, 则说明已经采到到了最后一个链接</returns>
		public bool IsTerminated(Response response)
		{
			var text = response?.Content?.ToString();
			if (string.IsNullOrWhiteSpace(text))
			{
				return false;
			}

			return !_unContains.All(c => text.Contains(c));
		}
	}
}

using System.Linq;
using DotnetSpider.Downloader;

namespace DotnetSpider.Core.Processor.TargetRequestExtractors
{
	/// <summary>
	/// 如果包含指定内容则到了最后一个采集链接
	/// </summary>
	public class ContainsTermination : ITargetRequestExtractorTermination
	{
		private readonly string[] _contains;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="contains">包含的内容</param>
		public ContainsTermination(string[] contains)
		{
			_contains = contains;
		}

		/// <summary>
		/// 是否到了最后一个链接
		/// </summary>
		/// <param name="response">链接请求结果</param>
		/// <returns>是否到了最终一个链接</returns>
		public bool IsTerminated(Response response)
		{
			var text = response?.Content == null ? "" : response.Content.ToString();
 
			return _contains.Any(c => text.Contains(c));
		}
	}

}

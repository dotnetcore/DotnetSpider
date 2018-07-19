using DotnetSpider.Common;
using DotnetSpider.Downloader.Redial;
using Newtonsoft.Json;
using System.Linq;

namespace DotnetSpider.Downloader.AfterDownloadCompleteHandlers
{
	/// <summary>
	/// Redial ADSL when <see cref="Response"/> contains specified contents.
	/// </summary>
	/// <summary xml:lang="zh-CN">
	/// 当包含指定内容时触发ADSL拨号
	/// </summary>
	public class RedialWhenContainsHandler : AfterDownloadCompleteHandler
	{
		private readonly string[] _contents;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="contents">包含的内容(specified contents to detect.)</param>
		public RedialWhenContainsHandler(params string[] contents)
		{
			_contents = contents;
		}

		/// <summary>
		/// Redial ADSL when <see cref="Response"/> contains specified contents.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 当包含指定内容时触发ADSL拨号
		/// </summary>
		/// <param name="response">页面数据 <see cref="Response"/></param>
		/// <param name="downloader">下载器 <see cref="IDownloader"/></param>
		public override void Handle(ref Response response, IDownloader downloader)
		{
			if (response == null || string.IsNullOrWhiteSpace(response.Content))
			{
				return;
			}
			var content = response.Content;
			var containContent = _contents.FirstOrDefault(c => content.Contains(c));

			if (containContent != null)
			{
				throw new NeedRedialException($"Download content contains: {JsonConvert.SerializeObject(_contents)}");
			}
		}
	}
}

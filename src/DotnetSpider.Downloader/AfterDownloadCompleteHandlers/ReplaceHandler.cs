using DotnetSpider.Common;

namespace DotnetSpider.Downloader.AfterDownloadCompleteHandlers
{
	/// <summary>
	/// Handler that replaces contents in <see cref="Response"/>.
	/// </summary>
	/// <summary xml:lang="zh-CN">
	/// 替换内容
	/// </summary>
	public class ReplaceHandler : AfterDownloadCompleteHandler
	{
		private readonly string _oldValue;
		private readonly string _newValue;

		/// <summary>
		/// Construct a ReplaceHandler
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 构造方法
		/// </summary>
		/// <param name="oldValue">The string to be replaced.</param>
		/// <param name="newValue">The string to replace all occurrences of oldValue.</param>
		public ReplaceHandler(string oldValue, string newValue = "")
		{
			_oldValue = oldValue;
			_newValue = newValue;
		}

		/// <summary>
		/// Replaces contents in <see cref="Response"/>.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 替换内容
		/// </summary>
		/// <param name="response">页面数据 <see cref="Response"/></param>
		/// <param name="downloader">下载器 <see cref="IDownloader"/></param>
		public override void Handle(ref Response response, IDownloader downloader)
		{
			if (response == null || string.IsNullOrWhiteSpace(response.Content))
			{
				return;
			}
			response.Content = response.Content.Replace(_oldValue, _newValue);
		}
	}
}

using DotnetSpider.Common;
using System.Text.RegularExpressions;

namespace DotnetSpider.Downloader.AfterDownloadCompleteHandlers
{
	/// <summary>
	/// Searches the current content for all occurrences of a specified regular expression, using the specified matching options.
	/// </summary>
	public class RegexHandler : AfterDownloadCompleteHandler
	{
		private readonly string _pattern;
		private readonly RegexOptions _regexOptions;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="pattern">The regular expression pattern to match.</param>
		/// <param name="options">A bitwise combination of the enumeration values that specify options for matching.</param>
		public RegexHandler(string pattern, RegexOptions options = RegexOptions.Multiline | RegexOptions.IgnoreCase)
		{
			_pattern = pattern;
			_regexOptions = options;
		}

		/// <summary>
		/// Searches the current content for all occurrences of a specified regular expression, using the specified matching options.
		/// </summary>
		/// <param name="response">页面数据 <see cref="Response"/></param>
		/// <param name="downloader">下载器 <see cref="IDownloader"/></param>
		public override void Handle(ref Response response, IDownloader downloader)
		{
			if (response == null || string.IsNullOrWhiteSpace(response.Content))
			{
				return;
			}

			string textValue = string.Empty;
			MatchCollection collection = Regex.Matches(response.Content, _pattern, _regexOptions);

			foreach (Match item in collection)
			{
				textValue += item.Value;
			}

			response.Content = textValue;
		}
	}
}

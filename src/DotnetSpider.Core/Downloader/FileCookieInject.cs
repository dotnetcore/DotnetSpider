using System.IO;

namespace DotnetSpider.Core.Downloader
{
	/// <summary>
	/// 从指定文件中读取Cookie注入到爬虫中
	/// </summary>
	public class FileCookieInject : CookieInjector
	{
		private readonly string _cookiePath;

		/// <summary>
		/// 构造方法
		/// </summary>
		public FileCookieInject()
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="path">Cookie文件路径</param>
		public FileCookieInject(string path)
		{
			if (!File.Exists(path))
			{
				throw new DownloadException("Cookie file unfound.");
			}
			_cookiePath = path;
		}

		protected override Cookies GetCookies(ISpider spider)
		{
			var path = string.IsNullOrEmpty(_cookiePath) ? $"{spider.Identity}.cookies" : _cookiePath;

			if (File.Exists(path))
			{
				var cookie = File.ReadAllText(path);
				return new Cookies
				{
					StringPart = cookie
				};
			}
			else
			{
				return new Cookies();
			}
		}
	}
}

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

		/// <summary>
		/// 取得新的Cookies
		/// </summary>
		/// <param name="spider">爬虫</param>
		/// <returns>Cookies</returns>
		protected override Cookies GetCookies(ISpider spider)
		{
			var path = string.IsNullOrEmpty(_cookiePath) ? $"{spider.Identity}.cookies" : _cookiePath;

			if (File.Exists(path))
			{
				var datas = File.ReadAllLines(path);
				if (datas.Length == 2)
				{
					var domain = datas[0];
					var cookiesStr = datas[1];
					var cookies = new Cookies();
					cookies.AddCookies(cookiesStr, domain);
					return cookies;
				}
				if (datas.Length == 3)
				{
					var domain = datas[0];
					var domainPath = datas[1];
					var cookiesStr = datas[2];
					var cookies = new Cookies();
					cookies.AddCookies(cookiesStr, domain, domainPath);
					return cookies;
				}
				return new Cookies();
			}
			else
			{
				return new Cookies();
			}
		}
	}
}

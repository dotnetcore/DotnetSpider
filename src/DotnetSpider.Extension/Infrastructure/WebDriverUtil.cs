using DotnetSpider.Core;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using System;
using System.IO;
using DotnetSpider.Common;
#if !NETSTANDARD
using System.Drawing;
#endif

namespace DotnetSpider.Extension.Infrastructure
{
	/// <summary>
	/// 创建WebDriver的选项
	/// </summary>
	public class Option
	{
		private string _proxy;

		/// <summary>
		/// 默认选项
		/// </summary>
		public static Option Default = new Option();

		/// <summary>
		/// 浏览器是否加载图片
		/// </summary>
		public bool LoadImage { get; set; } = true;

		/// <summary>
		/// 浏览器是否加载组件
		/// </summary>
		public bool AlwaysLoadNoFocusLibrary { get; set; } = true;

		/// <summary>
		/// 浏览器是否加载FlashPlayer
		/// </summary>
		public bool LoadFlashPlayer { get; set; } = true;

		/// <summary>
		/// 是否使用无头浏览器模式
		/// </summary>
		public bool Headless { get; set; }

		/// <summary>
		/// 使用的代理地址
		/// </summary>
		public string Proxy
		{
			get => _proxy;
			set
			{
				string v = value;
				if (string.IsNullOrEmpty(v))
				{
					_proxy = v;
				}
				else
				{
					string[] tmp = v.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

					if (tmp.Length == 2)
					{
						if (RegexUtil.IpAddress.IsMatch(tmp[0]) && int.TryParse(tmp[1], out _))
						{
							_proxy = v;
							return;
						}
					}

					throw new SpiderException("Proxy string should be like 192.168.1.100:8080");
				}
			}
		}

		/// <summary>
		/// The proxy authentication info (e.g. username:password).
		/// </summary>
		public string ProxyAuthentication;
	}

	/// <summary>
	/// WebDriver 帮助类
	/// </summary>
	public static class WebDriverUtil
	{
#if !NETSTANDARD
		/// <summary>
		/// 保存页面元素的内容为图片
		/// </summary>
		/// <param name="element">页面元素</param>
		/// <returns>图片</returns>
		public static Image ElementSnapshot(this IWebElement element)
		{
			Bitmap screenSnapshot = new Bitmap(element.Size.Width, element.Size.Height);
			Size size = new Size(Math.Min(element.Size.Width, screenSnapshot.Width),
				Math.Min(element.Size.Height, screenSnapshot.Height));
			Rectangle crop = new Rectangle(element.Location, size);
			return screenSnapshot.Clone(crop, screenSnapshot.PixelFormat);
		}
#endif
		/// <summary>
		/// 打开一个浏览器
		/// </summary>
		/// <param name="browser">浏览器</param>
		/// <param name="option">选项</param>
		/// <returns>WebDriver对象</returns>
		public static IWebDriver Open(Browser browser, Option option)
		{
			IWebDriver e = null;
			switch (browser)
			{
				case Browser.Firefox:
					string path = Environment.ExpandEnvironmentVariables("%APPDATA%") + @"\Mozilla\Firefox\Profiles\";
					string[] pathsToProfiles = Directory.GetDirectories(path, "*.webdriver", SearchOption.TopDirectoryOnly);
					var profile = pathsToProfiles.Length == 1 ? new FirefoxProfile(pathsToProfiles[0], false) : new FirefoxProfile();
					if (!option.AlwaysLoadNoFocusLibrary)
					{
						profile.AlwaysLoadNoFocusLibrary = true;
					}

					if (!option.LoadImage)
					{
						profile.SetPreference("permissions.default.image", 2);
					}
					if (!option.LoadFlashPlayer)
					{
						profile.SetPreference("dom.ipc.plugins.enabled.libflashplayer.so", "false");
					}
					if (!string.IsNullOrEmpty(option.Proxy))
					{
						string[] p = option.Proxy.Split(':');
						string host = p[0];
						int port = Convert.ToInt32(p[1]);
						profile.SetPreference("network.proxy.ftp_port", port);
						profile.SetPreference("network.proxy.gopher", host);
						profile.SetPreference("network.proxy.gopher_port", port);
						profile.SetPreference("network.proxy.http", host);
						profile.SetPreference("network.proxy.http_port", port);
						profile.SetPreference("network.proxy.no_proxies_on", "localhost,127.0.0.1,<-loopback>");
						profile.SetPreference("network.proxy.share_proxy_settings", true);
						profile.SetPreference("network.proxy.socks", host);
						profile.SetPreference("network.proxy.socks_port", port);
						profile.SetPreference("network.proxy.ssl", host);
						profile.SetPreference("network.proxy.ssl_port", port);
						profile.SetPreference("network.proxy.type", 1);
					}
					FirefoxOptions options = new FirefoxOptions();
					options.Profile = profile;
					e = new FirefoxDriver(options);
					break;
				case Browser.Chrome:
					ChromeDriverService cds = ChromeDriverService.CreateDefaultService(Env.BaseDirectory);
					cds.HideCommandPromptWindow = true;
					ChromeOptions opt = new ChromeOptions();
					if (!option.LoadImage)
					{
						opt.AddUserProfilePreference("profile", new { default_content_setting_values = new { images = 2 } });
					}
					if (!string.IsNullOrEmpty(option.Proxy))
					{
						opt.Proxy = new OpenQA.Selenium.Proxy() { HttpProxy = option.Proxy };
					}
					if (option.Headless)
					{
						opt.AddArgument("--headless");
					}
					e = new ChromeDriver(cds, opt);
					break;
			}
			return e;
		}
	}

}

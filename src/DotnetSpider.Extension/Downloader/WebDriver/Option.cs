using System;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core;

namespace DotnetSpider.Extension.Downloader.WebDriver
{
	public class Option
	{
		private string _proxy;

		public static Option Default = new Option();

		public bool LoadImage { get; set; } = true;

		public bool AlwaysLoadNoFocusLibrary { get; set; } = true;

		public bool LoadFlashPlayer { get; set; } = true;

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
						int port;
						if (RegexUtil.IpAddressRegex.IsMatch(tmp[0]) && int.TryParse(tmp[1], out port))
						{
							_proxy = v;
							return;
						}
					}

					throw new SpiderException("Proxy string should be like 192.168.1.100:8080.");
				}
			}
		}

		public string ProxyAuthentication { get; set; }
	}
}

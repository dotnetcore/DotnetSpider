using System;
using Java2Dotnet.Spider.Common;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Core.Utils;

namespace Java2Dotnet.Spider.Extension.Downloader.WebDriver
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
			get { return _proxy; }
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
						if (NetUtils.IpAddressRegex.IsMatch(tmp[0]) && int.TryParse(tmp[1], out port))
						{
							_proxy = v;
							return;
						}
					}

					throw new SpiderExceptoin("Proxy string should be like 192.168.1.100:8080.");
				}
			}
		}

		public string ProxyAuthentication { get; set; }
	}
}

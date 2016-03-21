using System;
using System.IO;
#if NET_CORE
using System.Runtime.InteropServices;
#endif

namespace Java2Dotnet.Spider.Common
{
	public class SpiderEnviroment
	{
		public static string GlobalDirectory;

		static SpiderEnviroment()
		{
#if !NET_CORE
			GlobalDirectory=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DotnetSpider"); 
#else
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				GlobalDirectory = Path.Combine("/usr/dotnetspider");
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				// 需要管理员帐户
				GlobalDirectory = Path.Combine("~/dotnetspider");
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				// 可能需要管理员帐户
				GlobalDirectory = @"C:\Program Files\DotnetSpider";
			}
			else
			{
				throw new ArgumentException("Unknow OS.");
			}

			DirectoryInfo di = new DirectoryInfo(GlobalDirectory);
			if (!di.Exists)
			{
				di.Create();
			}
#endif
		}
	}
}

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
		public static string BaseDirectory;

		static SpiderEnviroment()
		{
#if !NET_CORE
			GlobalDirectory=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DotnetSpider");
			BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
#else
			BaseDirectory = AppContext.BaseDirectory;
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				GlobalDirectory = Path.Combine(Environment.GetEnvironmentVariable("HOME"),"dotnetspider");
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				GlobalDirectory = Path.Combine(Environment.GetEnvironmentVariable("HOME"),"dotnetspider");
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				GlobalDirectory = $"C:\\Users\\{Environment.GetEnvironmentVariable("USERNAME")}\\Documents\\DotnetSpider\\";
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

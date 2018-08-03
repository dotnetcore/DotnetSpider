using System;
using System.IO;
#if NETSTANDARD || NETCOREAPP
using System.Runtime.InteropServices;
#endif

namespace DotnetSpider.Downloader
{
	public static class DownloaderEnv
	{
		public static string GlobalDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "dotnetspider");

#if NETFRAMEWORK
		public static string PathSeperator = "\\";
#else
		public static string PathSeperator = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\\" : "/";
#endif

#if NETFRAMEWORK
		public static bool IsWindows = true;
#else
		public static bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#endif
	}
}

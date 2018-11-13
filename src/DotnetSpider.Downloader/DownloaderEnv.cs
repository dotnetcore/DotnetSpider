using System;
using System.IO;
#if NETSTANDARD || NETCOREAPP
using System.Runtime.InteropServices;
#endif

namespace DotnetSpider.Downloader
{
	public static class DownloaderEnv
	{
		public static readonly string GlobalDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "dotnetspider");

#if NETFRAMEWORK
		public static string PathSeparator = "\\";
#else
		public static readonly string PathSeparator = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\\" : "/";
#endif

#if NETFRAMEWORK
		public static readonly bool IsWindows = true;
#else
		public static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#endif
	}
}

#if NET_CORE
using System.Runtime.InteropServices;
#endif

namespace DotnetSpider.Core.Infrastructure
{
	public class Environment
	{
		public static string PathSeperator;

		static Environment()
		{
#if !NET_CORE
			PathSeperator = "\\";
#else
			PathSeperator = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\\" : "/";
#endif
		}
	}
}

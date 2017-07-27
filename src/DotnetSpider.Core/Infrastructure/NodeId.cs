using System.IO;
#if NET_CORE
using System.Runtime.InteropServices;
#endif

namespace DotnetSpider.Core.Infrastructure
{
	public class NodeId
	{
		static NodeId()
		{
			string path = "";
#if NET_CORE
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{

			}
			path = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"c:\DotnetSpider\node.id" : "/opt/dotnetspider/node.id";

#else
			path = @"c:\DotnetSpider\node.id";
#endif
			if (File.Exists(path))
			{
				Id = File.ReadAllText(path);
				if (Id.Length > 100)
				{
					throw new SpiderException("Length of Node identity should less than 100.");
				}
			}
		}

		public static readonly string Id = "DEFAULT";
	}
}

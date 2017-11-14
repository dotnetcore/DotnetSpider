using System.IO;

#if NET_CORE
using System.Runtime.InteropServices;
#endif

namespace DotnetSpider.Core.Infrastructure
{
	public static class NodeId
	{
		public static readonly string Id;

		static NodeId()
		{
			Id = Env.Ip;
		}
	}
}

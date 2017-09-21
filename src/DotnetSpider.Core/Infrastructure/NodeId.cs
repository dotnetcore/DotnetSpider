using System;
using System.IO;
using System.Net;
#if NET_CORE
using System.Runtime.InteropServices;
#endif

namespace DotnetSpider.Core.Infrastructure
{
	public class NodeId
	{
		public static readonly string Id = "DEFAULT";

		static NodeId()
		{
			string path = Path.Combine(Env.GlobalDirectory, "node.id");

			if (!File.Exists(path))
			{
				Id = string.IsNullOrEmpty(Env.IP) ? Id : Env.IP;
				File.AppendAllText(path, Id);
			}
			else
			{
				Id = File.ReadAllText(path);
			}
			if (Id.Length > 100)
			{
				throw new SpiderException("Length of Node identity should less than 100.");
			}
		}
	}
}

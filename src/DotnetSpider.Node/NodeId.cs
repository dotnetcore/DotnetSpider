using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DotnetSpider.Node
{
	public static class NodeId
	{
		public readonly static string Id;

		static NodeId()
		{
			var file = "nodeid";
			if (!File.Exists(file))
			{
				Id = Guid.NewGuid().ToString("N");
				File.WriteAllText(file, Id);
			}
			else
			{
				Id = File.ReadAllText(file).Trim();
			}
		}
	}
}

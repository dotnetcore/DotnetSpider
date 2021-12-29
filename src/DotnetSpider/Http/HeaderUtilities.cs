using System.Collections.Generic;
using System.Text;

namespace DotnetSpider.Http
{
	public class HeaderUtilities
	{
		internal static void DumpHeaders(StringBuilder sb, params Dictionary<string, dynamic>[] headers)
		{
			sb.AppendLine("{");
			foreach (var t in headers)
			{
				if (t == null)
				{
					continue;
				}

				foreach (var keyValuePair in t)
				{
					sb.Append("  ");
					sb.Append(keyValuePair.Key);
					sb.Append(": ");
					sb.AppendLine(keyValuePair.Value);
				}
			}

			sb.Append('}');
		}
	}
}

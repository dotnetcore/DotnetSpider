using System.Collections.Generic;
using System.Text;

namespace DotnetSpider.Core.Downloader
{
	public class Cookies
	{
		public string StringPart { get; set; }
		public Dictionary<string, string> PairPart { get; set; } = new Dictionary<string, string>();

		public override string ToString()
		{
			StringBuilder builder = new StringBuilder("");

			foreach (var cookie in PairPart)
			{
				builder.Append($"{cookie.Key}={cookie.Value};");
			}

			builder.Append(StringPart);
			return builder.ToString();
		}
	}
}

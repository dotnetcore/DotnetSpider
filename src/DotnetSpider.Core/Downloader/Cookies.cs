using System.Collections.Generic;
using System.Text;

namespace DotnetSpider.Core.Downloader
{
	/// <summary>
	/// Cookie信息配置对象
	/// 完整的配置分为两部分, 一部分是StringPart, 另一部分为PairPart
	/// 大部分情况下可以直接设置StringPart为从Fiddler中拷贝完整的Cookie字符串
	/// </summary>
	public class Cookies
	{
		/// <summary>
		/// Cookie信息
		/// </summary>
		public string StringPart { get; set; }

		/// <summary>
		/// Cookie信息
		/// </summary>
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

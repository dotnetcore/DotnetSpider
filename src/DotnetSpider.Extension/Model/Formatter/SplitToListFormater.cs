using System;
using DotnetSpider.Core;
using System.Linq;

namespace DotnetSpider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property)]
	public class SplitToListFormater : Formatter
	{
		public string[] Splitors { get; set; }
		public int ElementAt { get; set; } = int.MaxValue;

		protected override dynamic FormateValue(dynamic value)
		{
			string tmp = value.ToString();
			string[] result = tmp.Split(Splitors, StringSplitOptions.RemoveEmptyEntries);

			return result.ToList();
		}

		protected override void CheckArguments()
		{
			if (Splitors == null || Splitors.Length == 0)
			{
				throw new SpiderException("Splitors should not be null or empty.");
			}

			if (ElementAt < 0)
			{
				throw new SpiderException("ElementAt should larger than 0.");
			}
		}
	}
}

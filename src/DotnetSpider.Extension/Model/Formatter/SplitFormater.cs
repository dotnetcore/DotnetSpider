using System;
using DotnetSpider.Core;

namespace DotnetSpider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property)]
	public class SplitFormatter : Formatter
	{
		public string[] Splitors { get; set; }
		public int ElementAt { get; set; }

		protected override dynamic FormateValue(dynamic value)
		{
			var result = value.Split(Splitors, StringSplitOptions.RemoveEmptyEntries);
			if (result != null)
			{
				if (result.Length > ElementAt)
				{
					return result[ElementAt];
				}
				else
				{
					return null;
				}
			}
			return null;
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

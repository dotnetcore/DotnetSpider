using System;
using DotnetSpider.Core;

namespace DotnetSpider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property)]
	public abstract class Formatter : System.Attribute, INamed
	{
		public string Name => GetType().Name;
		public string ValueWhenNull { get; set; } = null;

		public bool RetutnDateString { get; set; } = false;
		public DateTime DateTime { get; set; } = DateTime.Now;
		public string DateFormat { get; set; }

		protected abstract dynamic FormateValue(dynamic value);

		protected abstract void CheckArguments();

		public dynamic Formate(dynamic value)
		{
			CheckArguments();

			if (value == null)
			{
				return ValueWhenNull;
			}
			return FormateValue(value);
		}
	}
}

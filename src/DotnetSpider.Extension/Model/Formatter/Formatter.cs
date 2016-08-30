using System;
using DotnetSpider.Core;

namespace DotnetSpider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property)]
	public abstract class Formatter : System.Attribute, INamed
	{
		public string Name => GetType().Name;
		public string ValueWhenNull { get; set; } = string.Empty;

		protected abstract dynamic FormateValue(dynamic value);

		protected abstract void CheckArguments();

		public dynamic Formate(dynamic value)
		{
			CheckArguments();

			string tmp = value?.ToString();
			if (string.IsNullOrEmpty(tmp) || string.IsNullOrWhiteSpace(tmp))
			{
				return ValueWhenNull;
			}

			return FormateValue(tmp);
		}
	}
}

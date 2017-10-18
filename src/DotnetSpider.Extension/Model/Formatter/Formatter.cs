using System;
using DotnetSpider.Core;

namespace DotnetSpider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property)]
	public abstract class Formatter : System.Attribute, INamed
	{
		protected Formatter()
		{
			Name = GetType().Name;
		}

		public string Name { get; set; }

		public object ValueWhenNull { get; set; } 

		protected abstract object FormateValue(object value);

		protected abstract void CheckArguments();

		public object Formate(object value)
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

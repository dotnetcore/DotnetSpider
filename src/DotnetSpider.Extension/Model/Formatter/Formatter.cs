using System;
using DotnetSpider.Core;

namespace DotnetSpider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property)]
	public abstract class Formatter : System.Attribute, INamed
	{
		public string Name => GetType().Name;

		public abstract string Formate(string value);
	}
}

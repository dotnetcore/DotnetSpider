using System;

namespace DotnetSpider.Extension.Model.Attribute
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class EnviromentExtractBy : BaseExtractBy
	{
		public string Name { get; set; }
	}
}

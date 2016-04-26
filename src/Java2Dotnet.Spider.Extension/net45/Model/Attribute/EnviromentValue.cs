using System;

namespace Java2Dotnet.Spider.Extension.Model.Attribute
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class EnviromentExtractBy : BaseExtractBy
	{
		public string Name { get; set; }
	}
}

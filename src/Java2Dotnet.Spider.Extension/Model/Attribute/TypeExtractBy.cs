using System;

namespace Java2Dotnet.Spider.Extension.Model.Attribute
{
	[AttributeUsage(AttributeTargets.Class)]
	public class TypeExtractBy : BaseExtractBy
	{
		public bool Multi { get; set; } = false;
	}
}

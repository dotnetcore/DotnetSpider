using System;

namespace DotnetSpider.Extension.Model.Attribute
{
	[AttributeUsage(AttributeTargets.Class)]
	public class UpdateColumns : System.Attribute
	{
		public string[] Columns { get; set; }

		public UpdateColumns()
		{
		}

		public UpdateColumns(params string[] columns)
		{
			Columns = columns;
		}
	}
}

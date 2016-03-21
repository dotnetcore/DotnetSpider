using System;

namespace Java2Dotnet.Spider.Extension.Model.Attribute
{
	[AttributeUsage(AttributeTargets.Class)]
	public class Indexes : System.Attribute
	{
		/// <summary>
		/// 以,号分隔列名
		/// </summary>
		public string Primary { get; set; }
		/// <summary>
		/// 以,号分隔列名
		/// </summary>
		public string[] Index { get; set; }
		/// <summary>
		/// 以,号分隔列名
		/// </summary>
		public string[] Unique { get; set; }

		public string AutoIncrement { get; set; }
	}
}

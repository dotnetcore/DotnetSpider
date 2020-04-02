using System.Reflection;

namespace DotnetSpider.DataFlow.Storage
{
	/// <summary>
	/// 列信息
	/// </summary>
	public class Column
	{
		public string Name { get; set; }
		public string Type { get; set; }
		public int Length { get; set; } = 255;
		public bool Required { get; set; }

		/// <summary>
		/// 属性反射，用于设置解析值到实体对象
		/// </summary>
		public PropertyInfo PropertyInfo { get; set; }
	}
}
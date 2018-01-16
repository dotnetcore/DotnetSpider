using System;

namespace DotnetSpider.Extension.Model.Attribute
{
	/// <summary>
	/// 实体选择器
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class EntitySelector : SelectorAttribute
	{
		/// <summary>
		/// 从最终解析到的结果中取前 Take 个实体
		/// </summary>
		public int Take { get; set; } = -1;
	}
}

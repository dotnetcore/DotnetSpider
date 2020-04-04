using System;

namespace DotnetSpider.DataFlow.Parser
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class GlobalValueSelector : ValueSelector
	{
		/// <summary>
		/// 解析值的名称
		/// </summary>
		public string Name { get; set; }
	}
}

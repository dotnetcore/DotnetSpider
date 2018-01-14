using DotnetSpider.Core.Selector;

namespace DotnetSpider.Extension.Model.Attribute
{
	/// <summary>
	/// 选择器基本实现
	/// </summary>
	public class BaseSelector : Selector
	{
		/// <summary>
		/// 构造方法
		/// </summary>
		public BaseSelector()
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="expression">表达式</param>
		public BaseSelector(string expression) : base(expression)
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="type">选择器类型</param>
		/// <param name="expression">表达式</param>
		public BaseSelector(SelectorType type, string expression) : base(type, expression)
		{
		}

		/// <summary>
		/// 参数
		/// </summary>
		public string Argument { get; set; }
	}
}

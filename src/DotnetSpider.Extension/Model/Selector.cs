using DotnetSpider.Core.Selector;

namespace DotnetSpider.Extension.Model
{
	/// <summary>
	/// 选择器特性
	/// </summary>
	public class Selector : System.Attribute
	{
		/// <summary>
		/// 构造方法
		/// </summary>
		public Selector()
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="expression">表达式</param>
		public Selector(string expression)
		{
			Expression = expression;
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="type">选择器类型</param>
		/// <param name="expression">表达式</param>
		public Selector(SelectorType type, string expression)
		{
			Type = type;
			Expression = expression;
		}

		/// <summary>
		/// 选择器类型
		/// </summary>
		public SelectorType Type { get; set; } = SelectorType.XPath;

		/// <summary>
		/// 表达式
		/// </summary>
		public string Expression { get; set; }

		/// <summary>
		/// 参数
		/// </summary>
		public string Arguments { get; set; }
	}
}

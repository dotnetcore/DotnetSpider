using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DotnetSpider.Extraction.Model
{
	/// <summary>
	/// 选择器特性
	/// </summary>
	public class Selector : System.Attribute
	{
		[JsonIgnore]
		public override object TypeId => base.TypeId;

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
		public Selector(string expression) : this(expression, SelectorType.XPath)
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="expression">表达式</param>
		/// <param name="type">选择器类型</param>
		/// <param name="arguments">参数</param>
		public Selector(string expression, SelectorType type, string arguments = null)
		{
			Type = type;
			Expression = expression;
			Arguments = arguments;
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

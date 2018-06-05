using DotnetSpider.Core.Selector;

namespace DotnetSpider.Extension.Model
{

    /// <summary>
    /// 选择器特性接口
    /// </summary>
    public interface ISelectorAttribute
    {
        /// <summary>
        /// 选择器类型
        /// </summary>
        SelectorType Type { get; set; }

        /// <summary>
        /// 表达式
        /// </summary>
        string Expression { get; set; }

        /// <summary>
        /// 参数
        /// </summary>
        string Arguments { get; set; }
    }


    /// <summary>
    /// 选择器特性
    /// </summary>
    public class SelectorAttribute : System.Attribute, ISelectorAttribute
    {
        /// <summary>
        /// 构造方法
        /// </summary>
        public SelectorAttribute()
        {
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="expression">表达式</param>
        public SelectorAttribute(string expression)
        {
            Expression = expression;
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="type">选择器类型</param>
        /// <param name="expression">表达式</param>
        public SelectorAttribute(SelectorType type, string expression)
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

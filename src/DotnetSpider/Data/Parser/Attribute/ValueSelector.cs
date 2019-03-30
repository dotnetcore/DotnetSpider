using System.Reflection;
using DotnetSpider.Selector;

namespace DotnetSpider.Data.Parser.Attribute
{
    /// <summary>
    /// 属性选择器的定义
    /// </summary>
    public class ValueSelector : Selector
    {
        /// <summary>
        /// 属性反射，用于设置解析值到实体对象
        /// </summary>
        internal PropertyInfo PropertyInfo { get; set; }
        
        /// <summary>
        /// 值是否可以为空, 如果不能为空但解析到的值为空时，当前对象被抛弃
        /// </summary>
        internal  bool NotNull { get; set; }
        
        /// <summary>
        /// 解析值的名称，配置在 Entity 上时必填，配置在属性上时可以空，如果为空会被属性名替代
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 构造方法
        /// </summary>
        public ValueSelector()
        {
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="type">选择器类型</param>
        /// <param name="expression">表达式</param>
        public ValueSelector(string expression, SelectorType type = SelectorType.XPath)
            : base(expression, type)
        {
        }

        /// <summary>
        /// 数据格式化
        /// </summary>
        public Formatter.Formatter[] Formatters { get; set; }
        
        /// <summary>
        /// 额外选项的定义
        /// </summary>
        public ValueOption ValueOption { get; set; }
    }
}
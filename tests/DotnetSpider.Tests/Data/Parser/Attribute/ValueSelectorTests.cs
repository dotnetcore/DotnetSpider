using Xunit;

namespace DotnetSpider.Tests.Data.Parser.Attribute
{
    public class ValueSelectorTests
    {
        /// <summary>
        /// 校验 ValueSelector 的 PropertyInfo 值是否设置正确
        /// </summary>
        [Fact(DisplayName = "ValueSelectorPropertyInfo")]
        public void ValueSelectorPropertyInfo()
        {
            // TODO
        }
        
        /// <summary>
        /// 校验 ValueSelector 的 NotNull 值是否设置正确
        /// 如果属性上有 Required Attribute 则为 true, 无则为 false
        /// </summary>
        [Fact(DisplayName = "ValueSelectorNotNull")]
        public void ValueSelectorNotNull()
        {
            // TODO
        }
        
        /// <summary>
        /// 校验 ValueSelector 的 Name 值是否设置正确
        /// 配置在 Entity 上时必填，配置在属性上时可以空，如果为空会被属性名替代
        /// </summary>
        [Fact(DisplayName = "ValueSelectorName")]
        public void ValueSelectorName()
        {
            // TODO
        }
        
        /// <summary>
        /// 校验 ValueSelector 的 Formatter 值是否设置正确
        /// 1. 单个
        /// 2. 多个
        /// 3. 无
        /// </summary>
        [Fact(DisplayName = "ValueSelectorFormatter")]
        public void ValueSelectorFormatter()
        {
            // TODO
        }
        
        /// <summary>
        /// 校验 ValueSelector 的 ValueOption 值是否设置正确
        /// </summary>
        [Fact(DisplayName = "ValueSelectorValueOption")]
        public void ValueSelectorValueOption()
        {
            // TODO
        }
    }
}
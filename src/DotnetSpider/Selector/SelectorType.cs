using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DotnetSpider.Selector
{
    /// <summary>
    /// 查询器类型
    /// </summary>
    [Flags]
#if !NET451
	[JsonConverter(typeof(StringEnumConverter))]
#endif
    public enum SelectorType
    {
        /// <summary>
        /// XPath
        /// </summary>
        XPath,

        /// <summary>
        /// Regex
        /// </summary>
        Regex,

        /// <summary>
        /// Css
        /// </summary>
        Css,

        /// <summary>
        /// JsonPath
        /// </summary>
        JsonPath,

        /// <summary>
        /// Enviroment
        /// </summary>
        Enviroment
    }
}
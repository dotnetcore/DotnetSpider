namespace DotnetSpider.Selector
{
    /// <summary>
    /// 查询器类型
    /// </summary>
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
        /// Environment
        /// </summary>
        Environment
    }
}
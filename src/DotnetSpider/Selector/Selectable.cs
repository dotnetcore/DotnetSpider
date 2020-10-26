using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DotnetSpider.Selector
{
    /// <summary>
    /// 查询接口
    /// </summary>
    public abstract class Selectable : ISelectable
    {
        /// <summary>
        /// 查找所有的链接
        /// </summary>
        /// <returns>查询接口</returns>
        public abstract IEnumerable<string> Links();

        public abstract SelectableType Type { get; }

        /// <summary>
        /// 通过XPath查找结果
        /// </summary>
        /// <param name="xpath">XPath 表达式</param>
        /// <returns>查询接口</returns>
        public virtual ISelectable XPath(string xpath)
        {
            return Select(Selectors.XPath(xpath));
        }

        /// <summary>
        /// 通过Css 选择器查找元素, 并取得属性的值
        /// </summary>
        /// <param name="css">Css 选择器</param>
        /// <param name="attrName">查询到的元素的属性</param>
        /// <returns>查询接口</returns>
        public ISelectable Css(string css, string attrName)
        {
            return Select(Selectors.Css(css, attrName));
        }

        /// <summary>
        /// 通过JsonPath查找结果
        /// </summary>
        /// <param name="jsonPath">JsonPath 表达式</param>
        /// <returns>查询接口</returns>
        public virtual ISelectable JsonPath(string jsonPath)
        {
            return Select(Selectors.JsonPath(jsonPath));
        }

        /// <summary>
        /// 通过正则表达式查找结果
        /// </summary>
        /// <param name="pattern">正则表达式</param>
        /// <param name="options"></param>
        /// <param name="replacement"></param>
        /// <returns>查询接口</returns>
        public virtual ISelectable Regex(string pattern, RegexOptions options = RegexOptions.None, string replacement = "$0")
        {
            return Select(Selectors.Regex(pattern, options, replacement));
        }

        public abstract IEnumerable<ISelectable> Nodes();

        public abstract string Value { get; }

        /// <summary>
        /// 通过查询器查找结果
        /// </summary>
        /// <param name="selector">查询器</param>
        /// <returns>查询接口</returns>
        public abstract ISelectable Select(ISelector selector);

        /// <summary>
        /// 通过查询器查找结果
        /// </summary>
        /// <param name="selector">查询器</param>
        /// <returns>查询接口</returns>
        public abstract IEnumerable<ISelectable> SelectList(ISelector selector);
    }
}

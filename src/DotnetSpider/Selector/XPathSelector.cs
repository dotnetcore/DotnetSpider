using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace DotnetSpider.Selector
{
    /// <summary>
    /// Xpath 查询器
    /// </summary>
    public class XPathSelector : ISelector
    {
        private static readonly Regex AttributeXPathRegex =
            new Regex(@"@[\w\s-]+", RegexOptions.RightToLeft | RegexOptions.IgnoreCase);

        private readonly string _xpath;
        private readonly string _attrName;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="xpath">Xpath表达式</param>
        public XPathSelector(string xpath)
        {
            _xpath = xpath;

            var match = AttributeXPathRegex.Match(_xpath);
            if (!string.IsNullOrWhiteSpace(match.Value) && _xpath.EndsWith(match.Value))
            {
                _attrName = match.Value.Replace("@", "");
                _xpath = _xpath.Replace("/" + match.Value, "");
            }
        }

        /// <summary>
        /// 对节点进行查询, 查询结果为第一个符合查询条件的元素
        /// </summary>
        /// <param name="text">HTML元素</param>
        /// <returns>查询结果</returns>
        public ISelectable Select(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var document = new HtmlDocument {OptionAutoCloseOnEnd = true};
            document.LoadHtml(text);
            var node = document.DocumentNode.SelectSingleNode(_xpath);

            if (node == null)
            {
                return null;
            }

            if (HasAttribute)
            {
                return new TextSelectable(node.Attributes[_attrName]?.Value?.Trim());
            }
            else
            {
                return new HtmlSelectable(node);
            }
        }

        /// <summary>
        /// 对节点进行查询, 查询结果为所有符合查询条件的元素
        /// </summary>
        /// <param name="text">HTML元素</param>
        /// <returns>查询结果</returns>
        public IEnumerable<ISelectable> SelectList(string text)
        {
            var document = new HtmlDocument {OptionAutoCloseOnEnd = true};
            document.LoadHtml(text);
            var nodes = document.DocumentNode.SelectNodes(_xpath);
            if (nodes == null)
            {
                return null;
            }

            if (HasAttribute)
            {
                return nodes.Select(x => new TextSelectable(x.Attributes[_attrName]?.Value?.Trim()));
            }
            else
            {
                return nodes.Select(node => new HtmlSelectable(node));
            }
        }

        /// <summary>
        /// 判断查询是否包含属性
        /// </summary>
        /// <returns>如果返回 True, 则说明是查询元素的属性值</returns>
        public bool HasAttribute => !string.IsNullOrWhiteSpace(_attrName);

        public override int GetHashCode()
        {
            return $"{_xpath}{_attrName}".GetHashCode();
        }
    }
}
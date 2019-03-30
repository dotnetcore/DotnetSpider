using System.Collections.Generic;
using HtmlAgilityPack;

namespace DotnetSpider.Selector
{
    /// <summary>
    /// 查询接口
    /// </summary>
    public abstract class AbstractSelectable : ISelectable
    {
        /// <summary>
        /// 共享属性
        /// </summary>
        public Dictionary<string, dynamic> Properties { get; set; } = new Dictionary<string, dynamic>();

        /// <summary>
        /// 查找到的所有结果
        /// </summary>
        public List<dynamic> Elements { get; set; }

        /// <summary>
        /// 通过XPath查找结果
        /// </summary>
        /// <param name="xpath">XPath 表达式</param>
        /// <returns>查询接口</returns>
        public abstract ISelectable XPath(string xpath);

        /// <summary>
        /// 通过Css 选择器查找结果
        /// </summary>
        /// <param name="css">Css 选择器</param>
        /// <returns>查询接口</returns>
        public abstract ISelectable Css(string css);

        /// <summary>
        /// 通过Css 选择器查找元素, 并取得属性的值
        /// </summary>
        /// <param name="css">Css 选择器</param>
        /// <param name="attrName">查询到的元素的属性</param>
        /// <returns>查询接口</returns>
        public abstract ISelectable Css(string css, string attrName);

        /// <summary>
        /// 通过共用属性查找进村
        /// </summary>
        /// <param name="field">属性名称</param>
        /// <returns>查询结果</returns>
        public abstract dynamic Environment(string field);

        /// <summary>
        /// 查找所有的链接
        /// </summary>
        /// <returns>查询接口</returns>
        public abstract ISelectable Links();

        /// <summary>
        /// 取得查询器里所有的结果
        /// </summary>
        /// <returns>查询接口</returns>
        public abstract IEnumerable<ISelectable> Nodes();

        /// <summary>
        /// 通过JsonPath查找结果
        /// </summary>
        /// <param name="jsonPath">JsonPath 表达式</param>
        /// <returns>查询接口</returns>
        public abstract ISelectable JsonPath(string jsonPath);

        /// <summary>
        /// 通过正则表达式查找结果
        /// </summary>
        /// <param name="regex">正则表达式</param>
        /// <returns>查询接口</returns>
        public ISelectable Regex(string regex)
        {
            return Select(Selectors.Regex(regex));
        }

        /// <summary>
        /// 通过正则表达式查找结果
        /// </summary>
        /// <param name="regex">正则表达式</param>
        /// <param name="group">分组</param>
        /// <returns>查询接口</returns>
        public ISelectable Regex(string regex, int group)
        {
            return Select(Selectors.Regex(regex, group));
        }

        /// <summary>
        /// 获得当前查询器的文本结果, 如果查询结果为多个, 则返回第一个结果的值
        /// </summary>
        /// <param name="option">元素取值方式</param>
        /// <returns>查询到的文本结果</returns>
        public string GetValue(ValueOption option = ValueOption.None)
        {
            if (Elements == null || Elements.Count == 0)
            {
                return null;
            }

            var element = Elements[0];
            return CalculateValue(element, option);
        }

        /// <summary>
        /// 获得当前查询器的文本结果, 如果查询结果为多个, 则返回第一个结果的值
        /// </summary>
        /// <param name="option">元素取值方式</param>
        /// <returns>查询到的文本结果</returns>
        public IEnumerable<string> GetValues(ValueOption option = ValueOption.None)
        {
            List<string> result = new List<string>();
            foreach (var el in Elements)
            {
                var value = CalculateValue(el, option);
                if (!string.IsNullOrEmpty(value))
                {
                    result.Add(value);
                }
            }

            return result;
        }

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
        public abstract ISelectable SelectList(ISelector selector);

        private string CalculateValue(dynamic element, ValueOption option)
        {
            if (element is HtmlNode elementNode)
            {
                switch (option)
                {
                    case ValueOption.OuterHtml:
                    {
                        return elementNode.OuterHtml;
                    }
                    case ValueOption.InnerHtml:
                    {
                        return elementNode.InnerHtml;
                    }
                    case ValueOption.InnerText:
                    {
                        return elementNode.InnerText;
                    }
                    default:
                    {
                        return elementNode.InnerHtml;
                    }
                }
            }

            var document = new HtmlDocument();
            document.LoadHtml(element.ToString());

            switch (option)
            {
                case ValueOption.OuterHtml:
                {
                    return document.DocumentNode.OuterHtml;
                }
                case ValueOption.InnerHtml:
                {
                    return document.DocumentNode.InnerHtml;
                }
                case ValueOption.InnerText:
                {
                    // Cost too much, need re-implement

                    return document.DocumentNode.InnerText;
                }
                default:
                {
                    return element.ToString();
                }
            }
        }
    }
}
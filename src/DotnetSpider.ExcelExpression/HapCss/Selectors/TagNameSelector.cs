using System;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace DotnetSpider.ExcelExpression.HapCss.Selectors
{
    internal class TagNameSelector : CssSelector
    {
        public override string Token => string.Empty;

        protected internal override IEnumerable<HtmlNode> FilterCore(IEnumerable<HtmlNode> currentNodes)
        {
            foreach (var node in currentNodes)
            {
                if (node.Name.Equals(Selector, StringComparison.InvariantCultureIgnoreCase))
                    yield return node;
            }
        }
    }
}

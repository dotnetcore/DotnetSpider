using HtmlAgilityPack;
using System;
using System.Collections.Generic;

namespace DotnetSpider.Extraction.ExcelExpression.HapCss.Selectors
{
    internal class TagNameSelector : CssSelector
    {
        public override string Token
        {
            get { return string.Empty; }
        }

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

using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotnetSpider.Extraction.ExcelExpression.HapCss.Selectors
{
    internal class ClassNameSelector : CssSelector
    {
        public override string Token
        {
            get { return "."; }
        }

        protected internal override IEnumerable<HtmlNode> FilterCore(IEnumerable<HtmlNode> currentNodes)
        {
            foreach (var node in currentNodes)
            {
                if (node.GetClassList().Any(c => c.Equals(Selector, StringComparison.InvariantCultureIgnoreCase)))
                    yield return node;
            }
        }
    }
}

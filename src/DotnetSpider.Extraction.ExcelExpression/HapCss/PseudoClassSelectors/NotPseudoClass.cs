using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Extraction.ExcelExpression.HapCss.PseudoClassSelectors
{
    [PseudoClassName("not")]
    internal class NotPseudoClass : PseudoClass
    {
        protected override bool CheckNode(HtmlNode node, string parameter)
        {
            var selectors = CssSelector.Parse(parameter);
            var nodes = new[] { node };

            foreach (var selector in selectors)
                if (selector.FilterCore(nodes).Count() == 1)
                    return false;

            return true;
        }
    }
}
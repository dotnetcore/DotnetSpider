using HtmlAgilityPack;
using System;

namespace DotnetSpider.Extraction.ExcelExpression.HapCss.PseudoClassSelectors
{
    [PseudoClassName("nth-child")]
    internal class NthChildPseudoClass : PseudoClass
    {
        protected override bool CheckNode(HtmlNode node, string parameter)
        {
            return node.GetIndexOnParent() == int.Parse(parameter) - 1;
        }
    }
}
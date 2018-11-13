using HtmlAgilityPack;
using System.Collections.Generic;

namespace DotnetSpider.Extraction.ExcelExpression.HapCss.Selectors
{
    internal class AllSelector : CssSelector
    {
        public override string Token
        {
            get { return "*"; }
        }

        protected internal override IEnumerable<HtmlNode> FilterCore(IEnumerable<HtmlNode> currentNodes)
        {
            return currentNodes;
        }
    }
}
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;

namespace DotnetSpider.Extraction.ExcelExpression.HapCss.Selectors
{
    internal class ImediateChildrenSelector : CssSelector
    {
        public override bool AllowTraverse
        {
            get { return false; }
        }

        public override string Token
        {
            get { return ">"; }
        }

        protected internal override IEnumerable<HtmlNode> FilterCore(IEnumerable<HtmlNode> currentNodes)
        {
            return currentNodes.SelectMany(i => i.ChildNodes);
        }
    }
}
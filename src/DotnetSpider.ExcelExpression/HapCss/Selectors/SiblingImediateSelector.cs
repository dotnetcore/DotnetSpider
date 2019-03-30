using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace DotnetSpider.ExcelExpression.HapCss.Selectors
{
    internal class SiblingImediateSelector : CssSelector
    {
        public override bool AllowTraverse => false;

        public override string Token => "+";

        protected internal override IEnumerable<HtmlNode> FilterCore(IEnumerable<HtmlNode> currentNodes)
        {
            foreach (var node in currentNodes)
            {
                var idx = node.GetIndexOnParent();
                var n = node.ParentNode.ChildNodes.Where(i => i.NodeType == HtmlNodeType.Element).Skip(idx + 1).FirstOrDefault();

                if (n != null)
                    yield return n;
            }
        }
    }
}
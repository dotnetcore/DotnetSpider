using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotnetSpider.Extraction.ExcelExpression.HapCss.Selectors
{
    internal class SiblingImediateSelector : CssSelector
    {
        public override bool AllowTraverse
        {
            get { return false; }
        }

        public override string Token
        {
            get { return "+"; }
        }

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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace DotnetSpider.HtmlAgilityPack.Css
{
    #region Imports

	

	#endregion

    /// <summary>
    /// HtmlNode extension methods.
    /// </summary>
    public static class HtmlNodeExtensions
    {

        [Obsolete("Use LoadHtml() instead. The version of HtmlAgilityPack which is included in FizzlerEx does NOT suffer from the Form nesting bug.")]
        public static void LoadHtml2(this HtmlDocument document, string html)
        {
            document.LoadHtml(html);
        }

#if !NETFX_CORE
        [Obsolete("Use Load() instead. The version of HtmlAgilityPack which is included in FizzlerEx does NOT suffer from the Form nesting bug.")]
        public static void Load2(this HtmlDocument document, string path)
        {
            document.Load(path);
        }
#endif




        /// <summary>
        /// Determines whether this node is an element or not.
        /// </summary>
        public static bool IsElement(this HtmlNode node)
        {
            if (node == null) throw new ArgumentNullException("node");
            return node.NodeType == HtmlNodeType.Element;
        }

        /// <summary>
        /// Returns a collection of elements from this collection.
        /// </summary>
        public static IEnumerable<HtmlNode> Elements(this IEnumerable<HtmlNode> nodes)
        {
            if (nodes == null) throw new ArgumentNullException("nodes");
            return nodes.Where(n => n.IsElement());
        }

        /// <summary>
        /// Returns a collection of child nodes of this node.
        /// </summary>
        public static IEnumerable<HtmlNode> Children(this HtmlNode node)
        {
            if (node == null) throw new ArgumentNullException("node");
            return node.ChildNodes;
        }

        /// <summary>
        /// Returns a collection of child elements of this node.
        /// </summary>
        public static IEnumerable<HtmlNode> Elements(this HtmlNode node)
        {
            if (node == null) throw new ArgumentNullException("node");
            return node.Children().Elements();
        }

        /// <summary>
        /// Returns a collection of the sibling elements after this node.
        /// </summary>
        public static IEnumerable<HtmlNode> ElementsAfterSelf(this HtmlNode node)
        {
            if (node == null) throw new ArgumentNullException("node");
            return node.NodesAfterSelf().Elements();
        }

        /// <summary>
        /// Returns a collection of the sibling nodes after this node.
        /// </summary>
        public static IEnumerable<HtmlNode> NodesAfterSelf(this HtmlNode node)
        {
            if (node == null) throw new ArgumentNullException("node");
            return NodesAfterSelfImpl(node);
        }

        private static IEnumerable<HtmlNode> NodesAfterSelfImpl(HtmlNode node)
        {
            while ((node = node.NextSibling) != null)
                yield return node;
        }

        /// <summary>
        /// Returns a collection of the sibling elements before this node.
        /// </summary>
        public static IEnumerable<HtmlNode> ElementsBeforeSelf(this HtmlNode node)
        {
            if (node == null) throw new ArgumentNullException("node");
            return node.NodesBeforeSelf().Elements();
        }

        /// <summary>
        /// Returns a collection of the sibling nodes before this node.
        /// </summary>
        public static IEnumerable<HtmlNode> NodesBeforeSelf(this HtmlNode node)
        {
            if (node == null) throw new ArgumentNullException("node");
            return NodesBeforeSelfImpl(node);
        }

        private static IEnumerable<HtmlNode> NodesBeforeSelfImpl(HtmlNode node)
        {
            while ((node = node.PreviousSibling) != null)
                yield return node;
        }

        /// <summary>
        /// Returns a collection of nodes that contains this element 
        /// followed by all descendant nodes of this element.
        /// </summary>
        public static IEnumerable<HtmlNode> DescendantsAndSelf(this HtmlNode node)
        {
            if (node == null) throw new ArgumentNullException("node");
            return Enumerable.Repeat(node, 1).Concat(node.Descendants());
        }

        /// <summary>
        /// Returns a collection of all descendant nodes of this element.
        /// </summary>
        public static IEnumerable<HtmlNode> Descendants(this HtmlNode node)
        {
            if (node == null) throw new ArgumentNullException("node");
            return DescendantsImpl(node);
        }

        private static IEnumerable<HtmlNode> DescendantsImpl(HtmlNode node)
        {
            Debug.Assert(node != null);
            foreach (var child in node.ChildNodes)
            {
                yield return child;
                foreach (var descendant in child.Descendants())
                    yield return descendant;
            }
        }

        /// <summary>
        /// Returns a begin tag, including attributes, string 
        /// representation of this element.
        /// </summary>
        public static string GetBeginTagString(this HtmlNode node)
        {
            if (node == null) throw new ArgumentNullException("node");

            if (!node.IsElement())
                return string.Empty;

            var sb = new StringBuilder().Append('<').Append(node.Name);

            foreach (var attribute in node.Attributes)
            {
                sb.Append(' ')
                  .Append(attribute.Name)
                  .Append("=\"")
                  .Append(HtmlDocument.HtmlEncode(attribute.Value))
                  .Append('\"');
            }

            return sb.Append('>').ToString();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace DotnetSpider.HtmlAgilityPack.Css
{
    #region Imports

	

	#endregion

    /// <summary>
    /// An <see cref="IElementOps{TElement}"/> implementation for <see cref="HtmlNode"/>
    /// from <a href="http://www.codeplex.com/htmlagilitypack">HtmlAgilityPack</a>.
    /// </summary>
    public class HtmlNodeOps : IElementOps<HtmlNode>
    {

        public virtual Selector<HtmlNode> Type(NamespacePrefix prefix, string type)
        {
            return prefix.IsSpecific
                 ? (Selector<HtmlNode>)(nodes => Enumerable.Empty<HtmlNode>())
                 : (nodes => nodes.Elements().Where(n => n.Name == type));
        }

        public virtual Selector<HtmlNode> Universal(NamespacePrefix prefix)
        {
            return prefix.IsSpecific
                 ? (Selector<HtmlNode>)(nodes => Enumerable.Empty<HtmlNode>())
                 : (nodes => nodes.Elements());
        }

        public virtual Selector<HtmlNode> Id(string id)
        {
            return nodes =>
            {
                var element = nodes.Elements().FirstOrDefault(n => n.Id == id);
                return element != null ? new[] { element } : Enumerable.Empty<HtmlNode>();
            };
        }

        public virtual Selector<HtmlNode> Class(string clazz)
        {
            return nodes => nodes.Elements().Where(n => n.GetAttributeValue("class", string.Empty)
                                                         .Split(' ')
                                                         .Contains(clazz));
        }

        public virtual Selector<HtmlNode> AttributeExists(NamespacePrefix prefix, string name)
        {
            return prefix.IsSpecific
                 ? (Selector<HtmlNode>)(nodes => Enumerable.Empty<HtmlNode>())
                 : (nodes => nodes.Elements().Where(n => n.Attributes[name] != null));
        }

        public virtual Selector<HtmlNode> AttributeExact(NamespacePrefix prefix, string name, string value)
        {
            var withoutAttribute = string.IsNullOrEmpty(value);

            return prefix.IsSpecific
                 ? (Selector<HtmlNode>)(nodes => Enumerable.Empty<HtmlNode>())
                 : (nodes => from n in nodes.Elements()
                             let a = n.Attributes[name]
                             where withoutAttribute ? (a == null || string.IsNullOrEmpty(a.Value)) : (a != null && a.Value == value)
                             select n);
        }

        public virtual Selector<HtmlNode> AttributeNotEqual(NamespacePrefix prefix, string name, string value)
        {
            return prefix.IsSpecific
                 ? (Selector<HtmlNode>)(nodes => Enumerable.Empty<HtmlNode>())
                 : (nodes => from n in nodes.Elements()
                             let a = n.Attributes[name]
                             where a == null || a.Value != value
                             select n);
        }

        public virtual Selector<HtmlNode> AttributeIncludes(NamespacePrefix prefix, string name, string value)
        {
            return prefix.IsSpecific
                 ? (Selector<HtmlNode>)(nodes => Enumerable.Empty<HtmlNode>())
                 : (nodes => from n in nodes.Elements()
                             let a = n.Attributes[name]
                             where a != null && a.Value.Split(' ').Contains(value)
                             select n);
        }

        public virtual Selector<HtmlNode> AttributeRegexMatch(NamespacePrefix prefix, string name, string value)
        {
            var regex = CreateRegex(value);

            return prefix.IsSpecific
                 ? (Selector<HtmlNode>)(nodes => Enumerable.Empty<HtmlNode>())
                 : (nodes => from n in nodes.Elements()
                             let a = n.GetAttributeValue(name, string.Empty)
                             where regex.IsMatch(a)
                             select n);
        }

        public virtual Selector<HtmlNode> AttributeDashMatch(NamespacePrefix prefix, string name, string value)
        {
            return prefix.IsSpecific || string.IsNullOrEmpty(value)
                 ? (Selector<HtmlNode>)(nodes => Enumerable.Empty<HtmlNode>())
                 : (nodes => from n in nodes.Elements()
                             let a = n.Attributes[name]
                             where a != null && a.Value.Split('-').Contains(value)
                             select n);
        }

        public Selector<HtmlNode> AttributePrefixMatch(NamespacePrefix prefix, string name, string value)
        {
            return prefix.IsSpecific || string.IsNullOrEmpty(value)
                 ? (Selector<HtmlNode>)(nodes => Enumerable.Empty<HtmlNode>())
                 : (nodes => from n in nodes.Elements()
                             let a = n.Attributes[name]
                             where a != null && a.Value.StartsWith(value)
                             select n);
        }

        public Selector<HtmlNode> AttributeSuffixMatch(NamespacePrefix prefix, string name, string value)
        {
            return prefix.IsSpecific || string.IsNullOrEmpty(value)
                 ? (Selector<HtmlNode>)(nodes => Enumerable.Empty<HtmlNode>())
                 : (nodes => from n in nodes.Elements()
                             let a = n.Attributes[name]
                             where a != null && a.Value.EndsWith(value)
                             select n);
        }

        public Selector<HtmlNode> AttributeSubstring(NamespacePrefix prefix, string name, string value)
        {
            return prefix.IsSpecific || string.IsNullOrEmpty(value)
                 ? (Selector<HtmlNode>)(nodes => Enumerable.Empty<HtmlNode>())
                 : (nodes => from n in nodes.Elements()
                             let a = n.Attributes[name]
                             where a != null && a.Value.Contains(value)
                             select n);
        }

        public virtual Selector<HtmlNode> FirstChild()
        {
            return nodes => nodes.Where(n => !n.ElementsBeforeSelf().Any());
        }

        public virtual Selector<HtmlNode> LastChild()
        {
            return nodes => nodes.Where(n => n.ParentNode.NodeType != HtmlNodeType.Document
                                          && !n.ElementsAfterSelf().Any());
        }

        public virtual Selector<HtmlNode> NthChild(int a, int b)
        {
            if (a != 1)
                throw new NotSupportedException("The nth-child(an+b) selector where a is not 1 is not supported.");

            return nodes => from n in nodes
                            let elements = n.ParentNode.Elements().Take(b).ToArray()
                            where elements.Length == b && elements.Last().Equals(n)
                            select n;
        }

        public virtual Selector<HtmlNode> OnlyChild()
        {
            return nodes => nodes.Where(n => n.ParentNode.NodeType != HtmlNodeType.Document
                                          && !n.ElementsAfterSelf().Concat(n.ElementsBeforeSelf()).Any());
        }

        public virtual Selector<HtmlNode> Empty()
        {
            return nodes => nodes.Elements().Where(n => n.ChildNodes.Count == 0);
        }

        public virtual Selector<HtmlNode> Child()
        {
            return nodes => nodes.SelectMany(n => n.Elements());
        }

        public virtual Selector<HtmlNode> Descendant()
        {
            return nodes => nodes.SelectMany(n => n.Descendants().Elements());
        }

        public virtual Selector<HtmlNode> Adjacent()
        {
            return nodes => nodes.SelectMany(n => n.ElementsAfterSelf().Take(1));
        }

        public virtual Selector<HtmlNode> GeneralSibling()
        {
            return nodes => nodes.SelectMany(n => n.ElementsAfterSelf());
        }

        public Selector<HtmlNode> NthLastChild(int a, int b)
        {
            if (a != 1)
                throw new NotSupportedException("The nth-last-child(an+b) selector where a is not 1 is not supported.");

            return nodes => from n in nodes
                            let elements = n.ParentNode.Elements().Skip(Math.Max(0, n.ParentNode.Elements().Count() - b)).Take(b).ToArray()
                            where elements.Length == b && elements.First().Equals(n)
                            select n;
        }

        public Selector<HtmlNode> Eq(int n)
        {
            return nodes =>
            {
                var node = nodes.ElementAtOrDefault(n);
                return node != null ? new[] { node } : Enumerable.Empty<HtmlNode>();
            };
        }

        public Selector<HtmlNode> Has(ISelectorGenerator subgenerator)
        {
            var castedGenerator = (SelectorGenerator<HtmlNode>)subgenerator;

            var compiled = castedGenerator.Selector;

            return nodes => nodes.Where(n => compiled(new[] { n }).Any());
        }

        public Selector<HtmlNode> SplitAfter(ISelectorGenerator subgenerator)
        {
            return nodes => nodes.SelectMany(x => Split(subgenerator, x, false, true));
        }

        public Selector<HtmlNode> SplitBefore(ISelectorGenerator subgenerator)
        {
            return nodes => nodes.SelectMany(x => Split(subgenerator, x, true, false));
        }

        public Selector<HtmlNode> SplitBetween(ISelectorGenerator subgenerator)
        {
            return nodes => nodes.SelectMany(x => Split(subgenerator, x, false, false));
        }

        public Selector<HtmlNode> SplitAll(ISelectorGenerator subgenerator)
        {
            return nodes => nodes.SelectMany(x => Split(subgenerator, x, true, true));
        }

        private Selector<HtmlNode> GetSelector(ISelectorGenerator subgenerator)
        {
            return ((SelectorGenerator<HtmlNode>)subgenerator).Selector;
        }

        private IEnumerable<HtmlNode> Split(ISelectorGenerator subgenerator, HtmlNode parent, bool keepBefore, bool keepAfter)
        {
            var selector = GetSelector(subgenerator);

            var children = parent.ChildNodes.ToArray();
            var splitterPositions = new List<int>();
            var splitterIndex = 0;
            foreach (var splitter in selector(new[] { parent }))
            {
                splitterIndex = Array.IndexOf(children, splitter, splitterIndex);
                if (splitterIndex == -1)
                    throw new FormatException("The node splitter must be a direct child of the context node.");

                splitterPositions.Add(splitterIndex);
            }

            if (splitterPositions.Count == 0)
            {
                if (keepBefore && keepAfter)
                    yield return parent;
                yield break;
            }


            var doc = new HtmlDocument();
            var keepSeparators = keepBefore != keepAfter;


            if (keepBefore)
                yield return CreateNodesGroup(doc, children, 0, splitterPositions[0] + (keepSeparators ? 0 : -1));

            for (int i = 1; i < splitterPositions.Count; i++)
            {

                var indexBegin = splitterPositions[i - 1] + 1;
                var indexEnd = splitterPositions[i] - 1;

                if (keepSeparators)
                {
                    if (keepAfter) indexBegin--;
                    else indexEnd++;
                }

                yield return CreateNodesGroup(doc, children, indexBegin, indexEnd);
            }


            if (keepAfter)
                yield return CreateNodesGroup(doc, children, splitterPositions[splitterPositions.Count - 1] + (keepSeparators ? 0 : 1), children.Length - 1);

        }


        public Selector<HtmlNode> Before(ISelectorGenerator subgenerator)
        {
            var doc = new HtmlDocument();
            return nodes => nodes.SelectNonNull(parent =>
            {
                var end = IndexOfChild(subgenerator, parent, 0);
                return end != null ? CreateNodesGroup(doc, parent.ChildNodes, 0, end.Value - 1) : null;
            });
        }

        public Selector<HtmlNode> After(ISelectorGenerator subgenerator)
        {
            var doc = new HtmlDocument();
            return nodes => nodes.SelectNonNull(parent =>
            {
                var start = IndexOfChild(subgenerator, parent, 0);
                return start != null ? CreateNodesGroup(doc, parent.ChildNodes, start.Value + 1, parent.ChildNodes.Count - 1) : null;
            });
        }

        public Selector<HtmlNode> Between(ISelectorGenerator startGenerator, ISelectorGenerator endGenerator)
        {
            var doc = new HtmlDocument();
            return nodes => nodes.SelectNonNull(parent =>
            {
                var start = IndexOfChild(startGenerator, parent, 0);
                if (start == null) return null;
                var end = IndexOfChild(endGenerator, parent, start.Value);
                if (end == null) return null;

                return CreateNodesGroup(doc, parent.ChildNodes, start.Value + 1, end.Value - 1);
            });
        }

        private int? IndexOfChild(ISelectorGenerator subgenerator, HtmlNode parent, int startIndex)
        {
            var selector = GetSelector(subgenerator);

            var children = parent.ChildNodes;
            var limit = selector(new[] { parent })
                .Select(x => new { Node = x, Position = children.IndexOf(x) })
                .FirstOrDefault(x =>
                {
                    if (x.Position == -1)
                        throw new FormatException("The limit node must be a direct child of the context node.");
                    return x.Position >= startIndex;
                });

            return limit != null ? limit.Position : (int?)null;
        }

        private HtmlNode CreateNodesGroup(HtmlDocument doc, IList<HtmlNode> nodes, int start, int last)
        {
            var group = doc.CreateElement("fizzler_nodes_group");
            for (int i = start; i <= last; i++)
            {
                group.ChildNodes.Add(nodes[i]);
            }
            return group;
        }

        public Selector<HtmlNode> Not(ISelectorGenerator subgenerator)
        {
            var castedGenerator = (SelectorGenerator<HtmlNode>)subgenerator;

            var compiled = castedGenerator.Selector;

            return nodes =>
            {
                var matches = compiled(nodes.Select(x => x.ParentNode)).ToList();
                return nodes.Except(matches);
            };
        }

        public Selector<HtmlNode> SelectParent()
        {
            return nodes => nodes.SelectNonNull(x => x.ParentNode);
        }

        public Selector<HtmlNode> Contains(string text)
        {
            return nodes => nodes.Where(x => x.InnerText.Contains(text));
        }

        public Selector<HtmlNode> Matches(string pattern)
        {
            var regex = CreateRegex(pattern);
            return nodes => nodes.Where(x => regex.IsMatch(x.InnerText));
        }

        private static Regex CreateRegex(string pattern)
        {
            try
            {
                return new Regex(pattern, RegexOptions.Multiline | RegexOptions.Singleline);
            }
            catch (ArgumentException ex)
            {
                throw new FormatException(ex.Message, ex);
            }
        }

        public Selector<HtmlNode> Last()
        {
            return nodes =>
            {
                var last = nodes.LastOrDefault();
                return last != null ? new[] { last } : Enumerable.Empty<HtmlNode>();
            };
        }

    }
}

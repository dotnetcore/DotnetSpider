using System;
using System.Collections.Generic;
using System.Linq;

namespace DotnetSpider.HtmlAgilityPack.Css
{
    #region Imports

	

	#endregion

    /// <summary>
    /// A selector generator implementation for an arbitrary document/element system.
    /// </summary>
    public class SelectorGenerator<TElement> : ISelectorGenerator
    {
        private readonly IEqualityComparer<TElement> _equalityComparer;
        private readonly Stack<Selector<TElement>> _selectors;
        private bool _anchorToRoot;

        public SelectorGenerator(IElementOps<TElement> ops) : this(ops, null) { }

        public SelectorGenerator(IElementOps<TElement> ops, IEqualityComparer<TElement> equalityComparer)
        {
            if (ops == null) throw new ArgumentNullException("ops");
            Ops = ops;
            _equalityComparer = equalityComparer ?? EqualityComparer<TElement>.Default;
            _selectors = new Stack<Selector<TElement>>();
        }

        public Selector<TElement> Selector { get; private set; }
        object ISelectorGenerator.Selector => Selector;

        public IElementOps<TElement> Ops { get; private set; }

        public IEnumerable<Selector<TElement>> GetSelectors()
        {
            var selectors = _selectors;
            var top = Selector;
            return top == null
                 ? selectors.Select(s => s)
                 : selectors.Concat(Enumerable.Repeat(top, 1));
        }

        protected void Add(Selector<TElement> selector)
        {
            if (selector == null) throw new ArgumentNullException("selector");

            var top = Selector;
            Selector = top == null ? selector : (elements => selector(top(elements)));
        }

        public virtual void OnInit()
        {
            _selectors.Clear();
            Selector = null;
            _anchorToRoot = false;
        }

        public virtual void OnSelector()
        {
            if (Selector != null)
                _selectors.Push(Selector);
            Selector = null;
        }

        public virtual void OnClose()
        {
            var sum = GetSelectors().Aggregate((a, b) => (elements => a(elements).Concat(b(elements))));
            var normalize = _anchorToRoot ? (x => x) : Ops.Descendant();
            Selector = elements => sum(normalize(elements)).Distinct(_equalityComparer);
            _selectors.Clear();
        }

        public virtual void Id(string id)
        {
            Add(Ops.Id(id));
        }

        public virtual void Class(string clazz)
        {
            Add(Ops.Class(clazz));
        }

        public virtual void Type(NamespacePrefix prefix, string type)
        {
            Add(Ops.Type(prefix, type));
        }

        public virtual void Universal(NamespacePrefix prefix)
        {
            Add(Ops.Universal(prefix));
        }

        public virtual void AttributeExists(NamespacePrefix prefix, string name)
        {
            Add(Ops.AttributeExists(prefix, name));
        }

        public virtual void AttributeExact(NamespacePrefix prefix, string name, string value)
        {
            Add(Ops.AttributeExact(prefix, name, value));
        }

        public void AttributeNotEqual(NamespacePrefix prefix, string name, string value)
        {
            Add(Ops.AttributeNotEqual(prefix, name, value));
        }

        public virtual void AttributeIncludes(NamespacePrefix prefix, string name, string value)
        {
            Add(Ops.AttributeIncludes(prefix, name, value));
        }

        public virtual void AttributeRegexMatch(NamespacePrefix prefix, string name, string value)
        {
            Add(Ops.AttributeRegexMatch(prefix, name, value));
        }

        public virtual void AttributeDashMatch(NamespacePrefix prefix, string name, string value)
        {
            Add(Ops.AttributeDashMatch(prefix, name, value));
        }

        public void AttributePrefixMatch(NamespacePrefix prefix, string name, string value)
        {
            Add(Ops.AttributePrefixMatch(prefix, name, value));
        }

        public void AttributeSuffixMatch(NamespacePrefix prefix, string name, string value)
        {
            Add(Ops.AttributeSuffixMatch(prefix, name, value));
        }

        public void AttributeSubstring(NamespacePrefix prefix, string name, string value)
        {
            Add(Ops.AttributeSubstring(prefix, name, value));
        }

        public virtual void FirstChild()
        {
            Add(Ops.FirstChild());
        }

        public virtual void LastChild()
        {
            Add(Ops.LastChild());
        }

        public virtual void NthChild(int a, int b)
        {
            Add(Ops.NthChild(a, b));
        }

        public virtual void OnlyChild()
        {
            Add(Ops.OnlyChild());
        }

        public virtual void Empty()
        {
            Add(Ops.Empty());
        }

        public virtual void Child()
        {
            Add(Ops.Child());
        }

        public virtual void Descendant()
        {
            Add(Ops.Descendant());
        }

        public virtual void Adjacent()
        {
            Add(Ops.Adjacent());
        }

        public virtual void GeneralSibling()
        {
            Add(Ops.GeneralSibling());
        }

        public void NthLastChild(int a, int b)
        {
            Add(Ops.NthLastChild(a, b));
        }

        public void Eq(int n)
        {
            Add(Ops.Eq(n));
        }

        public void Has(ISelectorGenerator subgenerator)
        {
            Add(Ops.Has(subgenerator));
        }

        public void SplitAfter(ISelectorGenerator subgenerator)
        {
            Add(Ops.SplitAfter(subgenerator));
        }

        public void SplitBefore(ISelectorGenerator subgenerator)
        {
            Add(Ops.SplitBefore(subgenerator));
        }

        public void SplitBetween(ISelectorGenerator subgenerator)
        {
            Add(Ops.SplitBetween(subgenerator));
        }

        public void SplitAll(ISelectorGenerator subgenerator)
        {
            Add(Ops.SplitAll(subgenerator));
        }

        public void Before(ISelectorGenerator subgenerator)
        {
            Add(Ops.Before(subgenerator));
        }

        public void After(ISelectorGenerator subgenerator)
        {
            Add(Ops.After(subgenerator));
        }

        public void Between(ISelectorGenerator startGenerator, ISelectorGenerator endGenerator)
        {
            Add(Ops.Between(startGenerator, endGenerator));
        }

        public void Not(ISelectorGenerator subgenerator)
        {
            Add(Ops.Not(subgenerator));
        }

        public void SelectParent()
        {
            Add(Ops.SelectParent());
        }

        public void Contains(string text)
        {
            Add(Ops.Contains(text));
        }

        public void Matches(string regex)
        {
            Add(Ops.Matches(regex));
        }

        public void CustomSelector(object selector)
        {
            Add((Selector<TElement>)selector);
        }

        public ISelectorGenerator CreateNew()
        {
            return new SelectorGenerator<TElement>(Ops, _equalityComparer);
        }


        public void AnchorToRoot()
        {
            _anchorToRoot = true;
        }

        public void Last()
        {
            Add(Ops.Last());
        }
    }
}

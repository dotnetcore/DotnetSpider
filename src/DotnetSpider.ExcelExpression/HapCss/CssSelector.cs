using System;
using System.Collections.Generic;
using System.Linq;
using DotnetSpider.ExcelExpression.HapCss.Selectors;
using HtmlAgilityPack;

namespace DotnetSpider.ExcelExpression.HapCss
{
    public abstract class CssSelector
    {
        #region Constructor
        public CssSelector()
        {
            SubSelectors = new List<CssSelector>();
        }
        #endregion

        #region Properties
        private static readonly CssSelector[] SSelectors = FindSelectors();
        public abstract string Token { get; }
        protected virtual bool IsSubSelector => false;
        public virtual bool AllowTraverse => true;

        public IList<CssSelector> SubSelectors { get; set; }
        public string Selector { get; set; }
        #endregion

        #region Methods
        protected internal abstract IEnumerable<HtmlNode> FilterCore(IEnumerable<HtmlNode> currentNodes);

        public IEnumerable<HtmlNode> Filter(IEnumerable<HtmlNode> currentNodes)
        {
            var nodes = currentNodes;
            IEnumerable<HtmlNode> rt = FilterCore(nodes).Distinct();

            if (SubSelectors.Count == 0)
                return rt;

            foreach (var selector in SubSelectors)
                rt = selector.FilterCore(rt);

            return rt;
        }

        public virtual string GetSelectorParameter(string selector)
        {
            return selector.Substring(Token.Length);
        }

        public static IList<CssSelector> Parse(string cssSelector)
        {
            var rt = new List<CssSelector>();
            var tokens = Tokenizer.GetTokens(cssSelector);
            foreach (var token in tokens)
                rt.Add(ParseSelector(token));

            return rt;
        }

        private static CssSelector ParseSelector(Token token)
        {
            Type selectorType;
            CssSelector selector;

            if (char.IsLetter(token.Filter[0]))
                selector = SSelectors.First(i => i is TagNameSelector);
            else
                selector = SSelectors.Where(s => s.Token.Length > 0).FirstOrDefault(s => token.Filter.StartsWith(s.Token));

            if (selector == null)
                throw new InvalidOperationException("Token inválido: " + token.Filter);

            selectorType = selector.GetType();
            var rt = (CssSelector)Activator.CreateInstance(selectorType);

            string filter = token.Filter.Substring(selector.Token.Length);
            rt.SubSelectors = token.SubTokens.Select(i => ParseSelector(i)).ToList();

            rt.Selector = filter;
            return rt;
        }

        private static CssSelector[] FindSelectors()
        {
            var defaultAsm = typeof(CssSelector).Assembly;
            Func<Type, bool> typeQuery = type => type.IsSubclassOf(typeof(CssSelector)) && !type.IsAbstract;

            var defaultTypes = defaultAsm.GetTypes().Where(typeQuery);
            var types = AppDomain.CurrentDomain.GetAssemblies().Where(asm => asm == defaultAsm).SelectMany(asm => asm.GetTypes().Where(typeQuery));
            types = defaultTypes.Concat(types);

            var rt = types.Select(t => Activator.CreateInstance(t)).Cast<CssSelector>().ToArray();
            return rt;
        }

        #endregion
    }
}
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Extraction.ExcelExpression.HapCss
{
    public abstract class CssSelector
    {
        #region Constructor
        public CssSelector()
        {
            this.SubSelectors = new List<CssSelector>();
        }
        #endregion

        #region Properties
        private static readonly CssSelector[] s_Selectors = FindSelectors();
        public abstract string Token { get; }
        protected virtual bool IsSubSelector { get { return false; } }
        public virtual bool AllowTraverse { get { return true; } }

        public IList<CssSelector> SubSelectors { get; set; }
        public string Selector { get; set; }
        #endregion

        #region Methods
        protected internal abstract IEnumerable<HtmlNode> FilterCore(IEnumerable<HtmlNode> currentNodes);

        public IEnumerable<HtmlNode> Filter(IEnumerable<HtmlNode> currentNodes)
        {
            var nodes = currentNodes;
            IEnumerable<HtmlNode> rt = this.FilterCore(nodes).Distinct();

            if (this.SubSelectors.Count == 0)
                return rt;

            foreach (var selector in this.SubSelectors)
                rt = selector.FilterCore(rt);

            return rt;
        }

        public virtual string GetSelectorParameter(string selector)
        {
            return selector.Substring(this.Token.Length);
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
                selector = s_Selectors.First(i => i is Selectors.TagNameSelector);
            else
                selector = s_Selectors.Where(s => s.Token.Length > 0).FirstOrDefault(s => token.Filter.StartsWith(s.Token));

            if (selector == null)
                throw new InvalidOperationException("Token inválido: " + token.Filter);

            selectorType = selector.GetType();
            var rt = (CssSelector)Activator.CreateInstance(selectorType);

            string filter = token.Filter.Substring(selector.Token.Length);
            rt.SubSelectors = token.SubTokens.Select(i => CssSelector.ParseSelector(i)).ToList();

            rt.Selector = filter;
            return rt;
        }

        private static CssSelector[] FindSelectors()
        {
            var defaultAsm = typeof(CssSelector).Assembly;
            Func<Type, bool> typeQuery = type => type.IsSubclassOf(typeof(CssSelector)) && !type.IsAbstract;

            var defaultTypes = defaultAsm.GetTypes().Where(typeQuery);
            var types = System.AppDomain.CurrentDomain.GetAssemblies().Where(asm => asm == defaultAsm).SelectMany(asm => asm.GetTypes().Where(typeQuery));
            types = defaultTypes.Concat(types);

            var rt = types.Select(t => Activator.CreateInstance(t)).Cast<CssSelector>().ToArray();
            return rt;
        }

        #endregion
    }
}
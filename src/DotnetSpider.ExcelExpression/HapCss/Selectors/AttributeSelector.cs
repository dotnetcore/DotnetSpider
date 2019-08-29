using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace DotnetSpider.ExcelExpression.HapCss.Selectors
{
    internal class AttributeSelector : CssSelector
    {
        public override string Token => "[";

        protected internal override IEnumerable<HtmlNode> FilterCore(IEnumerable<HtmlNode> currentNodes)
        {
            var filter = GetFilter();
            foreach (var node in currentNodes)
            {
                if (filter(node))
                    yield return node;
            }
        }

        private Func<HtmlNode, bool> GetFilter()
        {
            var filter = Selector.Trim('[', ']');

            var idx = filter.IndexOf('=');

            if (idx == 0)
                throw new InvalidOperationException("Uso inválido de seletor por atributo: " + Selector);

            if (idx < 0)
                return node => node.Attributes.Contains(filter);

            var operation = GetOperation(filter[idx - 1]);

            if (!char.IsLetterOrDigit(filter[idx - 1]))
                filter = filter.Remove(idx - 1, 1);

            var values = filter.Split(new[] { '=' }, 2);
            filter = values[0];
            var value = values[1];

            if (value[0] == value[value.Length - 1] && (value[0] == '"' || value[0] == '\''))
                value = value.Substring(1, value.Length - 2);

            return node => node.Attributes.Contains(filter) && operation(node.Attributes[filter].Value, value);
        }

        private Func<string, string, bool> GetOperation(char value)
        {
            if (char.IsLetterOrDigit(value))
                return (attr, v) => attr == v;

            switch (value)
            {
                case '*': return (attr, v) => attr == v || attr.Contains(v);
                case '^': return (attr, v) => attr.StartsWith(v);
                case '$': return (attr, v) => attr.EndsWith(v);
                case '~': return (attr, v) => attr.Split(' ').Contains(v);
            }

            throw new NotSupportedException("Uso inválido de seletor por atributo: " + Selector);
        }
    }
}
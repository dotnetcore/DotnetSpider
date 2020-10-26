using System;
using System.Collections.Generic;
using System.Linq;
using DotnetSpider.Infrastructure;
using Newtonsoft.Json.Linq;

namespace DotnetSpider.Selector
{
    public class JsonSelectable : Selectable
    {
        private readonly JToken _token;

        public JsonSelectable(JToken token)
        {
            _token = token;
        }

        public override IEnumerable<string> Links()
        {
            throw new System.NotImplementedException();
        }

        public override IEnumerable<ISelectable> Nodes()
        {
            return _token.Children().Select(x => new JsonSelectable(x));
        }

        public override string Value => _token?.ToString();

        /// <summary>
        /// 通过查询器查找结果
        /// </summary>
        /// <param name="selector">查询器</param>
        /// <returns>查询接口</returns>
        public override ISelectable Select(ISelector selector)
        {
            selector.NotNull(nameof(selector));
            return selector.Select(_token.ToString());
        }

        /// <summary>
        /// 通过查询器查找结果
        /// </summary>
        /// <param name="selector">查询器</param>
        /// <returns>查询接口</returns>
        public override IEnumerable<ISelectable> SelectList(ISelector selector)
        {
            selector.NotNull(nameof(selector));
            return selector.SelectList(_token.ToString());
        }

        public override SelectableType Type => SelectableType.Json;
    }
}

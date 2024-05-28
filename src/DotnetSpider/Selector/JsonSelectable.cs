using System.Collections.Generic;
using System.Linq;
using DotnetSpider.Infrastructure;
using Newtonsoft.Json.Linq;

namespace DotnetSpider.Selector;

public class JsonSelectable(JToken token) : Selectable
{
    public override IEnumerable<string> Links()
    {
        throw new System.NotImplementedException();
    }

    public override IEnumerable<ISelectable> Nodes()
    {
        return token.Children().Select(x => new JsonSelectable(x));
    }

    public override string Value => token?.ToString();

    /// <summary>
    /// 通过查询器查找结果
    /// </summary>
    /// <param name="selector">查询器</param>
    /// <returns>查询接口</returns>
    public override ISelectable Select(ISelector selector)
    {
        selector.NotNull(nameof(selector));
        return selector.Select(token.ToString());
    }

    /// <summary>
    /// 通过查询器查找结果
    /// </summary>
    /// <param name="selector">查询器</param>
    /// <returns>查询接口</returns>
    public override IEnumerable<ISelectable> SelectList(ISelector selector)
    {
        selector.NotNull(nameof(selector));
        return selector.SelectList(token.ToString());
    }

    public override SelectableType Type => SelectableType.Json;
}

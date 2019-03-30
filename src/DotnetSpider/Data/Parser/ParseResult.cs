using System.Collections;
using System.Collections.Generic;
using DotnetSpider.Data.Storage.Model;

namespace DotnetSpider.Data.Parser
{
    public interface IParseResult : IEnumerable
    {
    }

    public class ParseResult<T> : List<T>, IParseResult where T : EntityBase<T>, new()
    {
    }
}
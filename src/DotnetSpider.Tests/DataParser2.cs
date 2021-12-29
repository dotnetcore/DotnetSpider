using DotnetSpider.DataFlow.Parser;
using DotnetSpider.DataFlow.Storage;

namespace DotnetSpider.Tests
{
	public class DataParser2<T>: DataParser<T> where T : EntityBase<T>, new()
	{
	}
}

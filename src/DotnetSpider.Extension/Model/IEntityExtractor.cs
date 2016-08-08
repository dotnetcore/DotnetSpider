using DotnetSpider.Core;

namespace DotnetSpider.Extension.Model
{
	public interface IEntityExtractor
	{
		dynamic Process(Page page);
		string EntityName { get; }
	}
}

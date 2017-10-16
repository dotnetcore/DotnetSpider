using System.Collections.Generic;
using DotnetSpider.Core;

namespace DotnetSpider.Extension.Model
{
	public interface IEntityExtractor<T>
	{
		IEntityDefine EntityDefine { get; }
		List<T> Extract(Page page);
		DataHandler<T> DataHandler { get; }
		string Name { get; }
	}
}

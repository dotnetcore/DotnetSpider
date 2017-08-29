using System.Collections.Generic;
using DotnetSpider.Core;

namespace DotnetSpider.Extension.Model
{
	public interface IEntityExtractor
	{
		List<DataObject> Extract(Page page);
		DataHandler DataHandler { get; }
		string Name { get; }
	}
}

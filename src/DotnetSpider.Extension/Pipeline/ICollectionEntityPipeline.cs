using System.Collections.Generic;

namespace DotnetSpider.Extension.Pipeline
{
	public interface ICollectionEntityPipeline
	{
		IEnumerable<dynamic> GetCollection(string entityName);
	}
}

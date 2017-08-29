using System.Collections.Generic;
using DotnetSpider.Extension.Model;

namespace DotnetSpider.Extension.Pipeline
{
	public interface ICollectEntityPipeline
	{
		List<DataObject> GetCollected(string entityName);
	}
}

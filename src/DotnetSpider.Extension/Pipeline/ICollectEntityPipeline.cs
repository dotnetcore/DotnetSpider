using System.Collections.Generic;

namespace DotnetSpider.Extension.Pipeline
{
	public interface ICollectEntityPipeline
	{
		List<dynamic> GetCollected(string entityName);
	}
}

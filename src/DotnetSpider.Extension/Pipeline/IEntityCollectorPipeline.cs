using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace DotnetSpider.Extension.Pipeline
{
	public interface IEntityCollectorPipeline : IEntityPipeline
	{
		IEnumerable<JObject> GetCollected();
	}
}

using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace DotnetSpider.Extension.Pipeline
{
	public interface ICollectEntityPipeline : IEntityPipeline
	{
		IEnumerable<JObject> GetCollected();
	}
}

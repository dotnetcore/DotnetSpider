using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Java2Dotnet.Spider.Extension.Pipeline
{
	public interface IEntityCollectorPipeline : IEntityPipeline
	{
		IEnumerable<JObject> GetCollected();
	}
}

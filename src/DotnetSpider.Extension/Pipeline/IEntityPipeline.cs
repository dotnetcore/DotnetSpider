using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace DotnetSpider.Extension.Pipeline
{
	public interface IEntityPipeline
	{
		void Process(string entityName, List<JObject> datas);
	}
}

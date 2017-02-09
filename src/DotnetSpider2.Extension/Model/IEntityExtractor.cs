using System.Collections.Generic;
using DotnetSpider.Core;
using Newtonsoft.Json.Linq;

namespace DotnetSpider.Extension.Model
{
	public interface IEntityExtractor
	{
		List<JObject> Extract(Page page);
		DataHandler DataHandler { get; }
		string Name { get; }
	}
}

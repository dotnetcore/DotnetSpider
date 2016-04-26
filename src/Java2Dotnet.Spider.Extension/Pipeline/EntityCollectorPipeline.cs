using System.Collections.Generic;
using Java2Dotnet.Spider.Core;
using Newtonsoft.Json.Linq;

namespace Java2Dotnet.Spider.Extension.Pipeline
{
	public class EntityCollectorPipeline : IEntityCollectorPipeline
	{
		private readonly List<JObject> _collector = new List<JObject>();

		public void Dispose()
		{
			_collector.Clear();
		}

		public IEnumerable<JObject> GetCollected()
		{
			return _collector;
		}

		public void Initialize()
		{
		}

		public void Process(List<JObject> datas, ISpider spider)
		{
			lock (this)
			{
				_collector.AddRange(datas);
			}
		}
	}
}

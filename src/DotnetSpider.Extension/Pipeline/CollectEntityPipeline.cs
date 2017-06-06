using System.Collections.Generic;
using DotnetSpider.Extension.Model;
using Newtonsoft.Json.Linq;

namespace DotnetSpider.Extension.Pipeline
{
	public class CollectEntityPipeline : BaseEntityPipeline, ICollectEntityPipeline
	{
		private readonly Dictionary<string, List<JObject>> _collector = new Dictionary<string, List<JObject>>();

		public override void Dispose()
		{
			_collector.Clear();
		}

		public List<JObject> GetCollected(string entityName)
		{
			lock (this)
			{
				List<JObject> result;
				if (_collector.TryGetValue(entityName, out result))
				{
					return result;
				}
			}
			return null;
		}

		public override void AddEntity(Entity metadata)
		{
		}

		public override void Process(string entityName, List<JObject> datas)
		{
			lock (this)
			{
				if (_collector.ContainsKey(entityName))
				{
					var list = _collector[entityName];
					list.AddRange(datas);
				}
				else
				{
					_collector.Add(entityName, new List<JObject>(datas));
				}
			}
		}
	}
}

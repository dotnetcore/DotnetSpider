using System.Collections.Generic;
using DotnetSpider.Extension.Model;

namespace DotnetSpider.Extension.Pipeline
{
	public class CollectEntityPipeline : BaseEntityPipeline, ICollectEntityPipeline
	{
		private readonly Dictionary<string, List<DataObject>> _collector = new Dictionary<string, List<DataObject>>();
		private readonly object _locker = new object();

		public override void Dispose()
		{
			_collector.Clear();
		}

		public List<DataObject> GetCollected(string entityName)
		{
			lock (_locker)
			{
				if (_collector.TryGetValue(entityName, out var result))
				{
					return result;
				}
			}
			return null;
		}

		public override void AddEntity(EntityDefine metadata)
		{
		}

		public override void Process(string entityName, List<DataObject> datas)
		{
			lock (_locker)
			{
				if (_collector.ContainsKey(entityName))
				{
					var list = _collector[entityName];
					list.AddRange(datas);
				}
				else
				{
					_collector.Add(entityName, new List<DataObject>(datas));
				}
			}
		}
	}
}

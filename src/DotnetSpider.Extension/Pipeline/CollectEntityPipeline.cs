using System.Collections.Generic;
using DotnetSpider.Extension.Model;

namespace DotnetSpider.Extension.Pipeline
{
	public class CollectEntityPipeline : BaseEntityPipeline, ICollectEntityPipeline
	{
		private readonly Dictionary<string, List<dynamic>> _collector = new Dictionary<string, List<dynamic>>();
		private readonly object _locker = new object();

		public override void Dispose()
		{
			_collector.Clear();
		}

		public List<dynamic> GetCollected(string entityName)
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

		internal override void AddEntity(IEntityDefine metadata)
		{
		}

		public override int Process (string entityName,List<dynamic> datas)
		{
			lock (_locker)
			{
				if (_collector.ContainsKey(entityName))
				{
					var list = _collector[entityName];
					foreach(var data in datas)
					{
						list.Add(data);
					}
				}
				else
				{
					var list = new List<dynamic>();
					foreach (var data in datas)
					{
						list.Add(data);
					}
					_collector.Add(entityName, list);
				}
				return datas.Count;
			}
		}
	}
}

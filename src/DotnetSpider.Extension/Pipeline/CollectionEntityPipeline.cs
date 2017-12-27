using System.Collections.Generic;
using System.Linq;
using DotnetSpider.Core;
using DotnetSpider.Extension.Model;

namespace DotnetSpider.Extension.Pipeline
{
	public class CollectionEntityPipeline : BaseEntityPipeline, ICollectionEntityPipeline
	{
		private readonly Dictionary<string, List<dynamic>> _collector = new Dictionary<string, List<dynamic>>();
		private readonly object _locker = new object();

		public override void Dispose()
		{
			_collector.Clear();
		}

		public IEnumerable<dynamic> GetCollection(string entityName)
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

		public override void AddEntity(IEntityDefine metadata)
		{
		}

		public override int Process (string entityName,IEnumerable<dynamic> datas, ISpider spider)
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
				return datas.Count();
			}
		}
	}
}

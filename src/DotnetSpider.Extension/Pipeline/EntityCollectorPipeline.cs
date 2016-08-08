using System;
using System.Collections.Generic;
using DotnetSpider.Core;
using Newtonsoft.Json.Linq;

namespace DotnetSpider.Extension.Pipeline
{
	public class EntityCollectorPipeline : IEntityCollectorPipeline
	{
		private readonly List<JObject> _collector = new List<JObject>();

		public ISpider Spider { get; protected set; }

		public void Dispose()
		{
			_collector.Clear();
		}

		public IEnumerable<JObject> GetCollected()
		{
			return _collector;
		}

		public void InitPipeline(ISpider spider)
		{
			Spider = spider;
		}

		public void Process(List<JObject> datas)
		{
			lock (this)
			{
				_collector.AddRange(datas);
			}
		}
	}
}

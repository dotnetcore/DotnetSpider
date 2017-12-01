using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Redial;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using DotnetSpider.Extension.Model;
using System.Collections.Concurrent;
using NLog;

namespace DotnetSpider.Extension.Pipeline
{
	public class HttpEntityPipeline : BaseEntityPipeline
	{
		private readonly string _api;

		internal ConcurrentDictionary<string, EntityAdapter> EntityAdapters { get; set; } = new ConcurrentDictionary<string, EntityAdapter>();

		public HttpEntityPipeline(string api)
		{
			_api = api;
		}

		public override void AddEntity(IEntityDefine entityDefine)
		{
			if (entityDefine == null)
			{
				throw new ArgumentException("Should not add a null entity to a entity dabase pipeline.");
			}

			if (entityDefine.TableInfo == null)
			{
				Logger.AllLog(Spider?.Identity, $"Schema is necessary, Skip {GetType().Name} for {entityDefine.Name}.", LogLevel.Warn);
				return;
			}

			EntityAdapter entityAdapter = new EntityAdapter(entityDefine.TableInfo, entityDefine.Columns);
			EntityAdapters.TryAdd(entityDefine.Name, entityAdapter);
		}

		public override int Process(string entityName, List<dynamic> datas)
		{
			if (datas == null || datas.Count == 0)
			{
				return 0;
			}
			int count = 0;

			if (EntityAdapters.TryGetValue(entityName, out var metadata))
			{
				var json = JsonConvert.SerializeObject(new
				{
					Database = metadata.Table.Database,
					Table = metadata.Table.CalculateTableName(),
					Items = datas
				});
				var content = new ByteArrayContent(Encoding.UTF8.GetBytes(json));
				NetworkCenter.Current.Execute("httpPipeline", () =>
				{
					var response = HttpSender.Client.PostAsync(_api, content).Result;
					response.EnsureSuccessStatusCode();
				});
			}
			return count;
		}
	}
}

using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Extension.Model;
using DotnetSpider.Core.Pipeline;
using System.Collections.Concurrent;
using System;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Infrastructure.Database;

namespace DotnetSpider.Extension.Pipeline
{
	public abstract class BaseEntityPipeline : BasePipeline, IEntityPipeline
	{
		internal ConcurrentDictionary<string, EntityAdapter> EntityAdapters { get; set; } = new ConcurrentDictionary<string, EntityAdapter>();

		public abstract int Process(string name, IEnumerable<dynamic> datas, ISpider spider);

		public override void Process(IEnumerable<ResultItems> resultItems, ISpider spider)
		{
			if (resultItems == null)
			{
				return;
			}

			foreach (var resultItem in resultItems)
			{
				int count = 0;
				int effectedRow = 0;
				foreach (var pair in resultItem.Results)
				{
					List<dynamic> list = new List<dynamic>();

					if (pair.Value is ISpiderEntity)
					{
						list.Add(pair.Value);
					}
					else
					{
						list.AddRange(pair.Value);
					}
					if (list.Count > 0)
					{
						count += list.Count;
						effectedRow += Process(pair.Key, list, spider);
					}
				}
				resultItem.Request.CountOfResults = count;
				resultItem.Request.EffectedRows = effectedRow;
			}
		}

		public virtual void AddEntity(IEntityDefine entityDefine)
		{
			if (entityDefine == null)
			{
				throw new ArgumentException("Should not add a null entity to a entity dabase pipeline.");
			}

			if (entityDefine.TableInfo == null)
			{
				Logger.Log($"Schema is necessary, Skip {GetType().Name} for {entityDefine.Name}.", Level.Warn);
				return;
			}

			EntityAdapter entityAdapter = new EntityAdapter(entityDefine.TableInfo, entityDefine.Columns);
			EntityAdapters.TryAdd(entityDefine.Name, entityAdapter);
		}

		public static IPipeline GetPipelineFromAppConfig()
		{
			if (Env.DataConnectionStringSettings == null)
			{
				return null;
			}
			IPipeline pipeline;
			switch (Env.DataConnectionStringSettings.ProviderName)
			{
				case DbProviderFactories.PostgreSqlProvider:
					{
						pipeline = new PostgreSqlEntityPipeline();
						break;
					}
				case DbProviderFactories.MySqlProvider:
					{
						pipeline = new MySqlEntityPipeline();
						break;
					}
				case DbProviderFactories.SqlServerProvider:
					{
						pipeline = new SqlServerEntityPipeline();
						break;
					}
				case "MongoDB":
					{
						pipeline = new MongoDbEntityPipeline(Env.DataConnectionString);
						break;
					}
				case "Cassandra":
					{
						pipeline = new CassandraEntityPipeline(Env.DataConnectionString);
						break;
					}
				case "HttpMySql":
					{
						pipeline = new HttpMySqlEntityPipeline();
						break;
					}
				default:
					{
						pipeline = new NullPipeline();
						break;
					}
			}
			return pipeline;
		}
	}
}

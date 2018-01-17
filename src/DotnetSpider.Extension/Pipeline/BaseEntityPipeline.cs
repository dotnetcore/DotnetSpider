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
	/// <summary>
	/// 爬虫实体类对应的数据管道
	/// </summary>
	public abstract class BaseEntityPipeline : BasePipeline, IEntityPipeline
	{
		/// <summary>
		/// 数据管道使用的实体中间信息
		/// </summary>
		internal ConcurrentDictionary<string, EntityAdapter> EntityAdapters { get; set; } = new ConcurrentDictionary<string, EntityAdapter>();

		/// <summary>
		/// 处理爬虫实体解析器解析到的实体数据结果
		/// </summary>
		/// <param name="name">爬虫实体类的名称</param>
		/// <param name="datas">实体类数据</param>
		/// <param name="spider">爬虫</param>
		/// <returns>最终影响结果数量(如数据库影响行数)</returns>
		public abstract int Process(string name, IEnumerable<dynamic> datas, ISpider spider);

		/// <summary>
		/// 处理页面解析器解析到的数据结果
		/// </summary>
		/// <param name="resultItems">数据结果</param>
		/// <param name="spider">爬虫</param>
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

		/// <summary>
		/// 添加爬虫实体类的定义
		/// </summary>
		/// <param name="entityDefine">爬虫实体类的定义</param>
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

		/// <summary>
		/// 从配置文件中获取默认的数据管道
		/// </summary>
		/// <returns>数据管道</returns>
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
						pipeline = new ConsolePipeline();
						break;
					}
			}
			return pipeline;
		}
	}
}

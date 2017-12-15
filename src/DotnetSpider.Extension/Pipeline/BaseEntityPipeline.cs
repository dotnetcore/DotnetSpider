using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Extension.Model;
using DotnetSpider.Core.Pipeline;

namespace DotnetSpider.Extension.Pipeline
{
	public abstract class BaseEntityPipeline : BasePipeline, IEntityPipeline
	{
		public abstract int Process(string name, List<dynamic> datas);

		public override void Process(IEnumerable<ResultItems> resultItems)
		{
			if (resultItems == null)
			{
				return;
			}

			foreach (var resultItem in resultItems)
			{
				int count = 0;
				int effectedRow = 0;
				foreach (var result in resultItem.Results)
				{
					List<dynamic> list = new List<dynamic>();
					dynamic data = resultItem.GetResultItem(result.Key);

					if (data is ISpiderEntity)
					{
						list.Add(data);
					}
					else
					{
						list.AddRange(data);
					}
					if (list.Count > 0)
					{
						count += list.Count;
						effectedRow += Process(result.Key, list);
					}
				}
				resultItem.Request.CountOfResults = count;
				resultItem.Request.EffectedRows = effectedRow;
			}
		}

		public abstract void AddEntity(IEntityDefine type);

		public static IPipeline GetPipelineFromAppConfig()
		{
			if (Env.DataConnectionStringSettings == null)
			{
				return null;
			}
			IPipeline pipeline;
			switch (Env.DataConnectionStringSettings.ProviderName)
			{
				case "Npgsql":
					{
						pipeline = new PostgreSqlEntityPipeline();
						break;
					}
				case "MySql.Data.MySqlClient":
					{
						pipeline = new MySqlEntityPipeline();
						break;
					}
				case "System.Data.SqlClient":
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

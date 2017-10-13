using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Extension.Model;
using System.Collections.Concurrent;
using NLog;
using DotnetSpider.Core.Redial;
using DotnetSpider.Extension.Infrastructure;
using System.Configuration;
using DotnetSpider.Core.Infrastructure.Database;
using DotnetSpider.Core.Pipeline;

namespace DotnetSpider.Extension.Pipeline
{
	public abstract class BaseEntityDbPipeline : BaseEntityPipeline
	{
		private ConnectionStringSettings _connectionStringSettings;
		private readonly string _connectString;
		private PipelineMode _defaultPipelineModel;

		protected abstract ConnectionStringSettings CreateConnectionStringSettings(string connectString = null);

		protected abstract DbParameter CreateDbParameter(string name, object value);
		protected abstract void InitAllSqlOfEntity(EntityAdapter adapter);

		public PipelineMode DefaultPipelineModel
		{
			get
			{
				return _defaultPipelineModel;
			}
			set
			{
				if (value == PipelineMode.Update)
				{
					throw new SpiderException("Can not set pipeline mode to Update.");
				}
				if (value != _defaultPipelineModel)
				{
					_defaultPipelineModel = value;
				}
			}
		}

		internal ConcurrentDictionary<string, EntityAdapter> EntityAdapters { get; set; } = new ConcurrentDictionary<string, EntityAdapter>();

		public IUpdateConnectString UpdateConnectString { get; set; }

		public ConnectionStringSettings ConnectionStringSettings
		{
			get
			{
				if (null == _connectionStringSettings)
				{
					if (string.IsNullOrEmpty(_connectString))
					{
						if (null == Env.DataConnectionStringSettings)
						{
							throw new SpiderException("Default DbConnection unfound.");
						}
						else
						{
							_connectionStringSettings = CreateConnectionStringSettings(Env.DataConnectionStringSettings?.ConnectionString);
						}
					}
					else
					{
						_connectionStringSettings = CreateConnectionStringSettings(_connectString);
					}
				}

				return _connectionStringSettings;
			}
			set => _connectionStringSettings = value;
		}

		protected BaseEntityDbPipeline(string connectString = null)
		{
			_connectString = connectString;
		}

		public override void AddEntity(EntityDefine entityDefine)
		{
			if (entityDefine == null)
			{
				throw new ArgumentException("Should not add a null entity to a entity dabase pipeline.");
			}

			if (entityDefine.TableInfo == null)
			{
				Logger.MyLog(Spider?.Identity, $"Schema is necessary, Skip {GetType().Name} for {entityDefine.Name}.", LogLevel.Warn);
				return;
			}

			EntityAdapter entityAdapter = new EntityAdapter(entityDefine.TableInfo, entityDefine.Columns);

			if (entityAdapter.Table.UpdateColumns != null && entityAdapter.Table.UpdateColumns.Length > 0)
			{
				entityAdapter.PipelineMode = PipelineMode.Update;
			}
			else
			{
				entityAdapter.PipelineMode = DefaultPipelineModel;
			}

			InitAllSqlOfEntity(entityAdapter);

			EntityAdapters.TryAdd(entityDefine.Name, entityAdapter);
		}

		public override void InitPipeline(ISpider spider)
		{
			if (ConnectionStringSettings == null)
			{
				if (UpdateConnectString == null)
				{
					throw new SpiderException("ConnectionStringSettings or IUpdateConnectString are unfound.");
				}
				else
				{
					for (int i = 0; i < 5; ++i)
					{
						try
						{
							ConnectionStringSettings = UpdateConnectString.GetNew();
							break;
						}
						catch (Exception e)
						{
							Logger.MyLog(Spider.Identity, "Update ConnectString failed.", LogLevel.Error, e);
							Thread.Sleep(1000);
						}
					}

					if (ConnectionStringSettings == null)
					{
						throw new SpiderException("Can not update ConnectionStringSettings via IUpdateConnectString.");
					}
				}
			}

			base.InitPipeline(spider);

			InitDatabaseAndTable();
		}

		internal abstract void InitDatabaseAndTable();

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
				default:
					{
						pipeline = new NullPipeline();
						break;
					}
			}
			return pipeline;
		}

		/// <summary>
		/// For test
		/// </summary>
		/// <returns></returns>
		internal string[] GetUpdateColumns(string entityName)
		{
			if (EntityAdapters.TryGetValue(entityName, out var metadata))
			{
				return metadata.Table.UpdateColumns;
			}
			return null;
		}
	}
}


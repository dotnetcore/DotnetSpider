using System;
using System.Threading;
using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Extension.Model;
using System.Collections.Concurrent;
using NLog;
using DotnetSpider.Extension.Infrastructure;
using System.Configuration;

namespace DotnetSpider.Extension.Pipeline
{
	public abstract class BaseEntityDbPipeline : BaseEntityPipeline
	{
		private ConnectionStringSettings _connectionStringSettings;
		private readonly string _connectString;
		private PipelineMode _defaultPipelineModel;

		protected abstract ConnectionStringSettings CreateConnectionStringSettings(string connectString = null);
		protected abstract void InitAllSqlOfEntity(EntityAdapter adapter);

		public PipelineMode DefaultPipelineModel
		{
			get => _defaultPipelineModel;
			set
			{
				if (value == PipelineMode.Update)
				{
					throw new SpiderException("Can not set pipeline mode to Update.");
				}
				if (!Equals(value, _defaultPipelineModel))
				{
					_defaultPipelineModel = value;
				}
			}
		}

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

		public override void Init()
		{
			base.Init();

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
							Logger.AllLog("Update ConnectString failed.", LogLevel.Error, e);
							Thread.Sleep(1000);
						}
					}

					if (ConnectionStringSettings == null)
					{
						throw new SpiderException("Can not update ConnectionStringSettings via IUpdateConnectString.");
					}
				}
			}

			InitDatabaseAndTable();
		}

		public override void AddEntity(IEntityDefine entityDefine)
		{
			base.AddEntity(entityDefine);

			var entityAdapter = EntityAdapters[entityDefine.Name];
			if (entityAdapter.Table.UpdateColumns != null && entityAdapter.Table.UpdateColumns.Length > 0)
			{
				entityAdapter.PipelineMode = PipelineMode.Update;
			}
			else
			{
				entityAdapter.PipelineMode = DefaultPipelineModel;
			}

			InitAllSqlOfEntity(entityAdapter);
		}

		internal abstract void InitDatabaseAndTable();

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

		private void CheckDbSettings()
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
							Logger.AllLog("Update ConnectString failed.", LogLevel.Error, e);
							Thread.Sleep(1000);
						}
					}

					if (ConnectionStringSettings == null)
					{
						throw new SpiderException("Can not update ConnectionStringSettings via IUpdateConnectString.");
					}
				}
			}
		}
	}
}


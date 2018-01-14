using System;
using System.Threading;
using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Infrastructure;
using System.Configuration;

namespace DotnetSpider.Extension.Pipeline
{
	/// <summary>
	/// 爬虫实体类对应的数据库的数据管道
	/// </summary>
	public abstract class BaseEntityDbPipeline : BaseEntityPipeline
	{
		private ConnectionStringSettings _connectionStringSettings;
		private readonly string _connectString;
		private PipelineMode _defaultPipelineModel;

		/// <summary>
		/// 通过连接字符串获取ConnectionStringSettings对象, 用于DbFactory生产IDbConnection
		/// </summary>
		/// <param name="connectString">连接字符器</param>
		/// <returns>ConnectionStringSettings对象</returns>
		protected abstract ConnectionStringSettings CreateConnectionStringSettings(string connectString = null);

		/// <summary>
		/// 初始化所有相关的SQL语句
		/// </summary>
		/// <param name="adapter">数据库管道使用的实体中间信息</param>
		protected abstract void InitAllSqlOfEntity(EntityAdapter adapter);

		/// <summary>
		/// 默认的数据管道模式
		/// </summary>
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

		/// <summary>
		/// 更新数据库连接字符串的接口, 如果自定义的连接字符串无法使用, 则会尝试通过更新连接字符串来重新连接
		/// </summary>
		public IUpdateConnectString UpdateConnectString { get; set; }

		/// <summary>
		/// ConnectionStringSettings对象, 用于DbFactory生产IDbConnection
		/// </summary>
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

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="connectString">数据库连接字符串</param>
		protected BaseEntityDbPipeline(string connectString = null)
		{
			_connectString = connectString;
		}

		/// <summary>
		/// 在使用数据管道前, 进行一些初始化工作, 不是所有的数据管道都需要进行初始化
		/// </summary>
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
							Logger.Log("Update ConnectString failed.", Level.Error, e);
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

		/// <summary>
		/// 添加爬虫实体类的定义
		/// </summary>
		/// <param name="entityDefine">爬虫实体类的定义</param>
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

		/// <summary>
		/// 初始化数据库和相关表
		/// </summary>
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
							Logger.Log("Update ConnectString failed.", Level.Error, e);
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


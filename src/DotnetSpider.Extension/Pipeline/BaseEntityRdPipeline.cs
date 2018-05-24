using System.Threading;
using DotnetSpider.Core;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Infrastructure;
using System.Configuration;
using Serilog;
using System.Data;
using DotnetSpider.Core.Infrastructure.Database;
using System.Data.Common;

namespace DotnetSpider.Extension.Pipeline
{
	/// <summary>
	/// 爬虫实体类对应的数据库的数据管道
	/// </summary>
	public abstract class BaseEntityRdPipeline : BaseEntityPipeline
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
					throw new SpiderException("Can not set pipeline mode to update");
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
		public IConnectionStringSettingsRefresher ConnectionStringSettingsRefresher { get; set; }

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
							throw new SpiderException("Default DbConnection unfound");
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
		/// <param name="pipelineMode">数据管道模式</param>
		protected BaseEntityRdPipeline(string connectString = null, PipelineMode pipelineMode = PipelineMode.Insert)
		{
			_connectString = connectString;
			DefaultPipelineModel = pipelineMode;
		}

		/// <summary>
		/// 在使用数据管道前, 进行一些初始化工作, 不是所有的数据管道都需要进行初始化
		/// </summary>
		public override void Init()
		{
			base.Init();

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

		protected DbConnection CreateDbConnection()
		{
			DbConnection conn = ConnectionStringSettings.CreateDbConnection();
			int i = 0;
			while (conn == null && ConnectionStringSettingsRefresher != null && i <= 5)
			{
				Thread.Sleep(50);
				lock (this)
				{
					ConnectionStringSettings = ConnectionStringSettingsRefresher.GetNew();
				}
				conn = ConnectionStringSettings.CreateDbConnection();
				++i;
			}
			return conn;
		}
	}
}


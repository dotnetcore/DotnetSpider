using DotnetSpider.Core;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extension.Processor;
using System.Linq;

namespace DotnetSpider.Extension
{
	/// <summary>
	/// 实体类爬虫的定义
	/// </summary>
	public abstract class EntitySpider : CommonSpider
	{
		/// <summary>
		/// 构造方法
		/// </summary>
		public EntitySpider() : this(new Site())
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="site">目标站点信息</param>
		public EntitySpider(Site site) : base(site)
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="name">爬虫名称</param>
		public EntitySpider(string name) : base(name)
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="name">爬虫名称</param>
		/// <param name="site">目标站点信息</param>
		public EntitySpider(string name, Site site) : base(name, site)
		{
		}

		/// <summary>
		/// 添加爬虫实体类
		/// </summary>
		/// <typeparam name="T">爬虫实体类的类型, 必须继承自 ISpiderEntity</typeparam>
		public void AddEntityType<T>() where T : ISpiderEntity
		{
			AddEntityType<T>(null, null, null);
		}

		/// <summary>
		/// 添加爬虫实体类
		/// </summary>
		/// <typeparam name="T">爬虫实体类的类型, 必须继承自 ISpiderEntity</typeparam>
		/// <param name="tableName">爬虫实体在数据库中的表名, 此优先级高于EntitySelector中的定义</param>
		public void AddEntityType<T>(string tableName) where T : ISpiderEntity
		{
			AddEntityType<T>(null, null, tableName);
		}

		/// <summary>
		/// 添加爬虫实体类
		/// </summary>
		/// <typeparam name="T">爬虫实体类的类型, 必须继承自 ISpiderEntity</typeparam>
		/// <param name="dataHandler">对解析的结果进一步加工操作</param>
		public void AddEntityType<T>(IDataHandler<T> dataHandler) where T : ISpiderEntity
		{
			AddEntityType(null, dataHandler, null);
		}

		/// <summary>
		/// 添加爬虫实体类
		/// </summary>
		/// <typeparam name="T">爬虫实体类的类型, 必须继承自 ISpiderEntity</typeparam>
		/// <param name="targetUrlsExtractor">目标链接的解析、筛选器</param>
		public void AddEntityType<T>(ITargetUrlsExtractor targetUrlsExtractor) where T : ISpiderEntity
		{
			AddEntityType<T>(targetUrlsExtractor, null, null);
		}

		/// <summary>
		/// 添加爬虫实体类
		/// </summary>
		/// <typeparam name="T">爬虫实体类的类型, 必须继承自 ISpiderEntity</typeparam>
		/// <param name="dataHandler">对解析的结果进一步加工操作</param>
		/// <param name="tableName">爬虫实体在数据库中的表名, 此优先级高于EntitySelector中的定义</param>
		public void AddEntityType<T>(IDataHandler<T> dataHandler, string tableName) where T : ISpiderEntity
		{
			AddEntityType(null, dataHandler, tableName);
		}

		/// <summary>
		/// 添加爬虫实体类
		/// </summary>
		/// <typeparam name="T">爬虫实体类的类型, 必须继承自 ISpiderEntity</typeparam>
		/// <param name="targetUrlsExtractor">目标链接的解析、筛选器</param>
		/// <param name="dataHandler">对解析的结果进一步加工操作</param>
		public void AddEntityType<T>(ITargetUrlsExtractor targetUrlsExtractor, IDataHandler<T> dataHandler) where T : ISpiderEntity
		{
			AddEntityType(targetUrlsExtractor, dataHandler, null);
		}

		/// <summary>
		/// 添加爬虫实体类
		/// </summary>
		/// <typeparam name="T">爬虫实体类的类型, 必须继承自 ISpiderEntity</typeparam>
		/// <param name="targetUrlsExtractor">目标链接的解析、筛选器</param>
		/// <param name="dataHandler">对解析的结果进一步加工操作</param>
		/// <param name="tableName">爬虫实体在数据库中的表名, 此优先级高于EntitySelector中的定义</param>
		public void AddEntityType<T>(ITargetUrlsExtractor targetUrlsExtractor, IDataHandler<T> dataHandler, string tableName) where T : ISpiderEntity
		{
			CheckIfRunning();

			EntityProcessor<T> processor = new EntityProcessor<T>(targetUrlsExtractor, dataHandler, tableName);
			AddPageProcessor(processor);
		}

		/// <summary>
		/// Get the default pipeline when user forget set a pepeline to spider.
		/// </summary>
		/// <returns>数据管道</returns>
		protected override IPipeline GetDefaultPipeline()
		{
			return BaseEntityPipeline.GetPipelineFromAppConfig();
		}

		/// <summary>
		/// 初始化数据管道
		/// </summary>
		/// <param name="arguments">运行参数</param>
		protected override void InitPipelines(params string[] arguments)
		{
			base.InitPipelines(arguments);

			if (!arguments.Contains("skip"))
			{
				var entityProcessors = PageProcessors.Where(p => p is IEntityProcessor).ToList();
				var entityPipelines = Pipelines.Where(p => p is BaseEntityPipeline).ToList();

				if (entityProcessors.Count != 0 && entityPipelines.Count == 0)
				{
					throw new SpiderException("You may miss a entity pipeline.");
				}
				foreach (var processor in entityProcessors)
				{
					foreach (var pipeline in entityPipelines)
					{
						var entityProcessor = processor as IEntityProcessor;
						if (pipeline is BaseEntityPipeline newPipeline)
						{
							if (entityProcessor != null)
							{
								newPipeline.AddEntity(entityProcessor.EntityDefine);
								newPipeline.Init();
							}
						}
					}
				}
			}
		}
	}
}
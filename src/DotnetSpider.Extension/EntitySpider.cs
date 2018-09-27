using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extension.Processor;
using DotnetSpider.Extraction.Model;
using System.Collections.Generic;

namespace DotnetSpider.Extension
{
	/// <summary>
	/// 实体类爬虫的定义
	/// </summary>
	public abstract class EntitySpider : DistributedSpider
	{
		private readonly Dictionary<string, ModelProcessor> _processors = new Dictionary<string, ModelProcessor>();

		/// <summary>
		/// 构造方法
		/// </summary>
		public EntitySpider() : this(null)
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="name">名称</param>
		/// <param name="site">目标站点信息</param>
		public EntitySpider(string name)
		{
			if (!string.IsNullOrWhiteSpace(name))
			{
				Name = name;
			}
		}

		/// <summary>
		/// 添加爬虫实体类
		/// </summary>
		/// <typeparam name="T">爬虫实体类的类型, 必须继承自 ISpiderEntity</typeparam>
		/// <param name="targetUrlsExtractor">目标链接的解析、筛选器</param>
		public EntityProcessor<T> AddEntityType<T>() where T : IBaseEntity
		{
			return AddEntityType<T>(null);
		}

		/// <summary>
		/// 添加爬虫实体类
		/// </summary>
		/// <typeparam name="T">爬虫实体类的类型, 必须继承自 ISpiderEntity</typeparam>
		/// <param name="targetUrlsExtractor">目标链接的解析、筛选器</param>
		/// <param name="dataHandler">对解析的结果进一步加工操作</param>
		public EntityProcessor<T> AddEntityType<T>(IDataHandler dataHandler) where T : IBaseEntity
		{
			CheckIfRunning();
			var typeName = typeof(T).FullName;
			if (!_processors.ContainsKey(typeName))
			{
				var processor = new EntityProcessor<T>(new ModelExtractor<T>(), dataHandler);
				_processors.Add(typeName, processor);
				AddPageProcessors(processor);
			}
			return _processors[typeName] as EntityProcessor<T>;
		}

		public ModelProcessor GetProcessor<T>()
		{
			var typeName = typeof(T).FullName;
			return _processors.ContainsKey(typeName) ? _processors[typeName] : null;
		}

		/// <summary>
		/// Get the default pipeline when user forget set a pepeline to spider.
		/// </summary>
		/// <returns>数据管道</returns>
		protected override IPipeline GetDefaultPipeline()
		{
			return DbEntityPipeline.GetPipelineFromAppConfig();
		}
	}
}
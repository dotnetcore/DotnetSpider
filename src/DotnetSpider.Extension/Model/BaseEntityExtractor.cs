using System.Collections.Generic;
using DotnetSpider.Core;

namespace DotnetSpider.Extension.Model
{
	/// <summary>
	/// 实体解析器的抽象
	/// </summary>
	/// <typeparam name="T">爬虫实体类的类型</typeparam>
	public abstract class BaseEntityExtractor<T> : IEntityExtractor<T>
	{
		/// <summary>
		/// 爬虫实体类的定义
		/// </summary>
		public IEntityDefine EntityDefine { get; }

		/// <summary>
		/// 解析器的名称, 默认值是爬虫实体类型的全称
		/// </summary>
		public string Name => EntityDefine.Name;

		/// <summary>
		/// 对解析的结果进一步加工操作
		/// </summary>
		public IDataHandler<T> DataHandler { get; }

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="dataHandler">对解析的结果进一步加工操作</param>
		/// <param name="tableName">实体在数据库中的表名, 此优先级高于EntitySelector中的定义</param>
		protected BaseEntityExtractor(IDataHandler<T> dataHandler = null, string tableName = null)
		{
			EntityDefine = new EntityDefine<T>();
			if (!string.IsNullOrWhiteSpace(tableName))
			{
				EntityDefine.TableInfo.Name = tableName;
			}
			DataHandler = dataHandler;
		}

		public abstract IEnumerable<T> Extract(Page page);

	}
}

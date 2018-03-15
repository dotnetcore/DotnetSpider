using Cassandra;
using DotnetSpider.Extension.Model.Attribute;
using System;

namespace DotnetSpider.Extension.Model
{
	/// <summary>
	/// 爬虫实体类接口
	/// </summary>
	public interface ISpiderEntity
	{
	}

	/// <summary>
	/// 爬虫实体类抽象
	/// </summary>
	public abstract class SpiderEntity : ISpiderEntity
	{
		/// <summary>
		/// 默认主键, 在插入数据的模式中, __Id 并没有什么作用. 在更新操作中, 需要把__id信息保存到Request的Extras中
		/// </summary>
		[PropertyDefine(Expression = "__id", Type = Core.Selector.SelectorType.Enviroment)]
		public long __Id { get; set; }

		/// <summary>
		/// 默认的创建时间
		/// </summary>
		[PropertyDefine(Expression = "now", Type = Core.Selector.SelectorType.Enviroment)]
		public DateTime CDate { get; set; } = DateTime.Now;
    }

	/// <summary>
	/// Cassandra专用的爬虫实体类
	/// </summary>
	public abstract class CassandraSpiderEntity : ISpiderEntity
	{
		/// <summary>
		/// 默认主键, 在插入数据的模式中, Id 并没有什么作用. 在更新操作中, 需要把Id信息保存到Request的Extras中
		/// </summary>
		[PropertyDefine(Expression = "Id", Type = Core.Selector.SelectorType.Enviroment)]
		public TimeUuid Id { get; set; }

        /// <summary>
        /// 默认的创建时间
        /// </summary>
        [PropertyDefine(Expression = "now", Type = Core.Selector.SelectorType.Enviroment)]
        public DateTime CDate { get; set; } = DateTime.Now;
	}
}

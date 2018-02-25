using System.Collections.Generic;
using DotnetSpider.Core;

namespace DotnetSpider.Extension.Model
{
	/// <summary>
	/// 爬虫实体的解析器
	/// </summary>
	/// <typeparam name="T">爬虫实体类的类型</typeparam>
	public interface IEntityExtractor<T>
	{
		/// <summary>
		/// 爬虫实体的定义
		/// </summary>
		IEntityDefine EntityDefine { get; }

		/// <summary>
		/// 解析成爬虫实体对象
		/// </summary>
		/// <param name="page">页面数据</param>
		/// <returns>爬虫实体对象</returns>
		IEnumerable<T> Extract(Page page);

		/// <summary>
		/// 对Processor的结果进一步加工操作
		/// </summary>
		IDataHandler<T> DataHandler { get; }

		/// <summary>
		/// 解析器的名称, 默认值是爬虫实体类型的全称
		/// </summary>
		string Name { get; }
	}
}

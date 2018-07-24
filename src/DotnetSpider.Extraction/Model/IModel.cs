using DotnetSpider.Extraction.Model.Attribute;
using System.Collections.Generic;

namespace DotnetSpider.Extraction.Model
{
	/// <summary>
	/// 爬虫数据模型的定义
	/// </summary>
	public interface IModel
	{
		/// <summary>
		/// 数据模型的选择器
		/// </summary>
		Selector Selector { get; }

		/// <summary>
		/// 从最终解析到的结果中取前 Take 个实体
		/// </summary>
		int Take { get; }

		/// <summary>
		/// 设置 Take 的方向, 默认是从头部取
		/// </summary>
		bool TakeFromHead { get; }

		/// <summary>
		/// 爬虫实体对应的数据库表信息
		/// 允许 TableInfo 为空, 有可能是临时数据并不需要落库的
		/// </summary>
		TableInfo Table { get; }

		/// <summary>
		/// 爬虫实体定义的数据库列信息
		/// </summary>
		HashSet<FieldSelector> Fields { get; }

		/// <summary>
		/// 目标链接的选择器
		/// </summary>
		IEnumerable<TargetRequestSelector> TargetRequestSelectors { get; }

		/// <summary>
		/// 共享值的选择器
		/// </summary>
		IEnumerable<SharedValueSelector> SharedValueSelectors { get; }

		string Identity { get; }
	}
}
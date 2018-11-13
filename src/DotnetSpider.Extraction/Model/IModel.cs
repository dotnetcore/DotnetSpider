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
		/// 爬虫实体定义的数据库列信息
		/// </summary>
		HashSet<Field> Fields { get; }

		/// <summary>
		/// 目标链接的选择器
		/// </summary>
		IEnumerable<Target> Targets { get; }

		/// <summary>
		/// 共享值的选择器
		/// </summary>
		IEnumerable<Share> Shares { get; }

		string Identity { get; }
	}
}
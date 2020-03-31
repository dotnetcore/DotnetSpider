using System.Collections.Generic;
using System.Linq;

namespace DotnetSpider.DataFlow.Storage
{
	/// <summary>
	/// 表元数据
	/// </summary>
	public class TableMetadata
	{
		/// <summary>
		/// 实体类型名称
		/// </summary>
		public string TypeName { get; set; }

		/// <summary>
		/// Schema
		/// </summary>
		public Schema Schema { get; set; }

		/// <summary>
		/// 主键
		/// </summary>
		public HashSet<string> Primary { get; set; }

		/// <summary>
		/// 索引
		/// </summary>
		public HashSet<IndexMetadata> Indexes { get; }

		/// <summary>
		/// 更新列
		/// </summary>
		public HashSet<string> Updates { get; set; }

		/// <summary>
		/// 属性名，属性数据类型的字典
		/// </summary>
		public Dictionary<string, Column> Columns { get; }

		/// <summary>
		/// 是否是自增主键
		/// </summary>
		public bool IsAutoIncrementPrimary => Primary != null && Primary.Count == 1 &&
		                                      (Columns[Primary.First()].Type == "Int32" ||
		                                       Columns[Primary.First()].Type == "Int64");

		/// <summary>
		/// 判断某一列是否在主键中
		/// </summary>
		/// <param name="column">列</param>
		/// <returns></returns>
		public bool IsPrimary(string column)
		{
			return Primary != null && Primary.Contains(column);
		}

		/// <summary>
		/// 判断是否有主键
		/// </summary>
		public bool HasPrimary => Primary != null && Primary.Count > 0;

		/// <summary>
		/// 判断是否有更新列
		/// </summary>
		public bool HasUpdateColumns => Updates != null && Updates.Count > 0;

		/// <summary>
		/// 构造方法
		/// </summary>
		public TableMetadata()
		{
			Indexes = new HashSet<IndexMetadata>();
			Columns = new Dictionary<string, Column>();
			Primary = new HashSet<string>();
			Updates = new HashSet<string>();
		}
	}
}
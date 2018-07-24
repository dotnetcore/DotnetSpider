using System.Collections.Generic;

namespace DotnetSpider.Extension.Pipeline
{
	/// <summary>
	/// 数据结果的收集接口
	/// </summary>
	public interface ICollectionEntityPipeline
	{
		/// <summary>
		/// 取得实体名称 entityName 的所有数据结果
		/// </summary>
		/// <param name="entityName">实体名称</param>
		/// <returns>数据结果</returns>
		IList<dynamic> GetCollection(string entityName);
	}
}

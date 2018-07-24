using System.Collections.Generic;

namespace DotnetSpider.Core.Pipeline
{
	/// <summary>
	/// 所有数据结果存在内存中.
	/// </summary>
	public interface ICollectionPipeline : IPipeline
	{
		/// <summary>
		/// Get all results collected.
		/// </summary>
		/// <param name="owner">数据拥有者</param>
		/// <returns>All results collected</returns>
		IList<ResultItems> GetCollection(dynamic owner);
	}
}

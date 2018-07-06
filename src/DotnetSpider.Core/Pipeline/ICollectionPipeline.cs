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
		/// <returns>All results collected</returns>
		IEnumerable<ResultItems> GetCollection(ISpider spider);
	}
}

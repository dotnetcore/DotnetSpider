using System.Collections;
using System.Collections.Generic;

namespace DotnetSpider.Core.Pipeline
{
	/// <summary>
	/// Pipeline that can collect and store results.
	/// </summary>
	public interface ICollectionPipeline : IPipeline
	{
		/// <summary>
		/// Get all results collected.
		/// </summary>
		/// <returns></returns>
		IEnumerable<ResultItems> GetCollection();
	}
}

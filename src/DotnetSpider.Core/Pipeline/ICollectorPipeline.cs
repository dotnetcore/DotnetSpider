using System.Collections;

namespace DotnetSpider.Core.Pipeline
{
	/// <summary>
	/// Pipeline that can collect and store results.
	/// </summary>
	public interface ICollectorPipeline : IPipeline
	{
		/// <summary>
		/// Get all results collected.
		/// </summary>
		/// <returns></returns>
		IEnumerable GetCollected();
	}
}

using System.Collections.Generic;

namespace DotnetSpider.Core.Pipeline
{
	/// <summary>
	/// Write results in console.
	/// Usually used in test.
	/// </summary>
	public class ConsolePipeline : BasePipeline
	{
		public override void Process(IEnumerable<ResultItems> resultItems)
		{
			foreach (var resultItem in resultItems)
			{
				foreach (var entry in resultItem)
				{
					System.Console.WriteLine(entry.Key + ":\t" + entry.Value);
				}
			}
		}
	}
}

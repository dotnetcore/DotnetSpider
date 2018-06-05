using System.Collections.Generic;

namespace DotnetSpider.Core.Pipeline
{
	/// <summary>
	/// 打印数据结果到控制台
	/// </summary>
	public class ConsolePipeline : BasePipeline
	{
		/// <summary>
		/// 打印数据结果到控制台
		/// </summary>
		/// <param name="resultItems">数据结果</param>
		/// <param name="spider">爬虫</param>
		public override void Process(IEnumerable<ResultItems> resultItems, ISpider spider)
		{
			foreach (var resultItem in resultItems)
			{
				resultItem.Request.CountOfResults = 0;
				resultItem.Request.EffectedRows = 0;

				foreach (var entry in resultItem.Results)
				{
					System.Console.WriteLine(entry.Key + ":\t" + entry.Value);

					resultItem.Request.CountOfResults += 1;
					resultItem.Request.EffectedRows += 1;
				}
			}
		}
	}
}

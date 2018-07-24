using DotnetSpider.Common;
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
		/// <param name="logger">日志接口</param>
		/// <param name="sender">调用方</param>
		public override void Process(IList<ResultItems> resultItems, ILogger logger, dynamic sender = null)
		{
			foreach (var resultItem in resultItems)
			{
				foreach (var entry in resultItem.Results)
				{
					System.Console.WriteLine(entry.Key + ":\t" + entry.Value);

					resultItem.Request.AddCountOfResults(1);
					resultItem.Request.AddEffectedRows(1);
				}
			}
		}
	}
}

using System;
using System.Collections.Generic;

namespace DotnetSpider.Core.Pipeline
{
	/// <summary>
	/// 默认空的数据管道
	/// </summary>
	public class NullPipeline : BasePipeline
	{
		/// <summary>
		/// 不作任何处理
		/// </summary>
		/// <param name="resultItems">数据结果</param>
		/// <param name="spider">爬虫</param>
		public override void Process(IEnumerable<ResultItems> resultItems, ISpider spider)
		{
			Console.WriteLine("You used a null pipeline.");
		}
	}
}

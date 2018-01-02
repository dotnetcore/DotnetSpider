using System;
using System.Collections.Generic;

namespace DotnetSpider.Core.Pipeline
{
	/// <summary>
	/// 默认空的数据管道
	/// </summary>
	public class NullPipeline : BasePipeline
	{
		public override void Process(IEnumerable<ResultItems> resultItems, ISpider spider)
		{
			Console.WriteLine("You used a null pipeline.");
		}
	}
}

using System;
using System.Collections.Generic;

namespace DotnetSpider.Core.Pipeline
{
	public class NullPipeline : BasePipeline
	{
		public override void Process(IEnumerable<ResultItems> resultItems)
		{
			Console.WriteLine("You used a null pipeline.");
		}
	}
}

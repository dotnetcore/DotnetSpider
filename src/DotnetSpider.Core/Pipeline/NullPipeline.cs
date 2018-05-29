using System;
using System.Collections.Generic;

namespace DotnetSpider.Core.Pipeline
{
	public class NullPipeline : BasePipeline
	{
        public override void Process(IList<ResultItems> resultItems, ISpider spider)
        {
			Console.WriteLine("You used a null pipeline.");
		}
	}
}

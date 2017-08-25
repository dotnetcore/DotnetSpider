using System;

namespace DotnetSpider.Core.Pipeline
{
	public class NullPipeline : BasePipeline
	{
		public override void Process(params ResultItems[] resultItems)
		{
			Console.WriteLine("You used a null pipeline.");
		}
	}
}

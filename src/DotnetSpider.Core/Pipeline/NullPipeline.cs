using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

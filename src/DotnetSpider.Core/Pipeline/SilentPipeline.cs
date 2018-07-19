using System.Collections.Generic;
using DotnetSpider.Common;

namespace DotnetSpider.Core.Pipeline
{
	public class SilentPipeline : IPipeline
	{
		public void Dispose()
		{
		}

		public void Process(IEnumerable<ResultItems> resultItems, ILogger logger, dynamic sender = null)
		{
		}
	}
}

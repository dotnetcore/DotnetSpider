using System.Collections.Generic;
using DotnetSpider.Common;

namespace DotnetSpider.Core.Pipeline
{
	public class SilentPipeline : IPipeline
	{
		public void Dispose()
		{
		}

		public void Process(IList<ResultItems> resultItems, ILogger logger, dynamic sender = null)
		{
		}
	}
}

using DotnetSpider.Core.Infrastructure;
using NLog;
using System.Collections.Generic;
using System.IO;
#if NET_CORE
#endif

namespace DotnetSpider.Core.Pipeline
{
	public abstract class BasePipeline : IPipeline
	{
		protected static readonly ILogger Logger = LogCenter.GetLogger();

		public abstract void Process(IEnumerable<ResultItems> resultItems, ISpider spider);

		public virtual void Init() { }

		public virtual void Dispose()
		{
		}
	}
}

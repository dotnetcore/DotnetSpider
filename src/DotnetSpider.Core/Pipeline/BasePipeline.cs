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

		public ISpider Spider { get; protected set; }

		public virtual void Init(ISpider spider)
		{
			Spider = spider;
		}

		public abstract void Process(IEnumerable<ResultItems> resultItems);

		public virtual void Dispose()
		{
		}
	}
}

using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotnetSpider.DataFlow;
using DotnetSpider.DataFlow.Storage;
using Geekbuying.HBaseClient;
using Polly;

namespace DotnetSpider.HBase
{
	public class HBaseEntityStorage : EntityStorageBase
	{
		protected override Task<DataFlowResult> Store(DataFlowContext context)
		{
			return null;
		}
	}
}
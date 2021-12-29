using System;
using System.Threading.Tasks;

namespace DotnetSpider.Proxy
{
	public class FakeProxyValidator : IProxyValidator
	{
		public Task<bool> IsAvailable(Uri proxy)
		{
			return Task.FromResult(true);
		}
	}
}

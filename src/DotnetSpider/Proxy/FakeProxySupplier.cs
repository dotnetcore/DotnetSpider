using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DotnetSpider.Proxy
{
	public class FakeProxySupplier : IProxySupplier
	{
		private int _port = 80;

		public Task<IEnumerable<Uri>> GetProxiesAsync()
		{
			Interlocked.Increment(ref _port);
			var array = (new[] {new Uri($"http://localhost:{_port}")}).AsEnumerable();
			return Task.FromResult(array);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace DotnetSpider.Proxy
{
	internal class EmptyProxyService : IProxyService
	{
		public Task<Uri> GetAsync(int seconds)
		{
			throw new NotImplementedException();
		}

		public Uri Get()
		{
			throw new NotImplementedException();
		}

		public Task ReturnAsync(Uri proxy, HttpStatusCode statusCode)
		{
			throw new NotImplementedException();
		}

		public Task<int> AddAsync(IEnumerable<Uri> proxies)
		{
			throw new NotImplementedException();
		}
	}
}

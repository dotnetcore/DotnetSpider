using DotnetSpider.Broker.Controllers;
using DotnetSpider.Broker.Services;
using DotnetSpider.Common;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace DotnetSpider.Broker.Test
{
	public class RequestQueueServiceTest : BaseTest
	{
		public RequestQueueServiceTest()
		{
			var options = new BrokerOptions
			{
				ConnectionString = "Server=.\\SQLEXPRESS;Database=DotnetSpider_Dev;Integrated Security = SSPI;",
				StorageType = StorageType.SqlServer,
				Tokens = new HashSet<string> { "aaa" },
				UseToken = false
			};
			Init(options);
		}

		[Fact(DisplayName = "Add")]
		public async void Add()
		{
			var service = Services.GetRequiredService<IRequestQueueService>();

			var r1 = new Request { Url = "http://www.a.com/1", Content = "1" };
			var r2 = new Request { Url = "http://www.a.com/2", Content = "2" };
			var identity = Guid.NewGuid().ToString("N");
			var blockId = await service.Add(identity, JsonConvert.SerializeObject(new List<Request> { r1, r2 }));
			List<RequestQueue> list = (await service.GetByBlockId(blockId)).ToList();
			var requests = list.Select(r => JsonConvert.DeserializeObject<Request>(r.Request)).ToList();

			Assert.Equal(2, requests.Count);
			Assert.Contains(requests, t => t.Url == "http://www.a.com/1");
			Assert.Contains(requests, t => t.Url == "http://www.a.com/2");
			Assert.Contains(requests, t => t.Content == "1");
			Assert.Contains(requests, t => t.Content == "2");
		}
	}
}

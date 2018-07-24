using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using Xunit;

namespace DotnetSpider.Common.Test
{
	public class RequestTest
	{
		public static Request GetRequest()
		{
			var extras = new Dictionary<string, dynamic> { { "Test", "Forever" } };
			var request = new Request("http://www.taobao.com", extras)
			{
				Method = HttpMethod.Get,
				Priority = 1
			};
			return request;
		}

		[Fact(DisplayName = "Request")]
		public void Request()
		{
			var request = GetRequest();
			Assert.Single(request.Properties);
			Assert.Equal(request.Properties["Test"], "Forever");
		}

		[Fact(DisplayName = "Request_PutExtra")]
		public void PutExtra()
		{
			var request = GetRequest();
			request.AddProperty(null, null);
			request.AddProperty("", null);
			request.AddProperty("", "");
			request.AddProperty("", "");
			request.AddProperty("One", "One");
			request.AddProperty("One", "One");
			Assert.Equal(3, request.Properties.Count);
			Assert.Equal(request.Properties["One"], "One");
			Assert.Equal(request.Properties[""], "");
		}

		[Fact(DisplayName = "Request_GetExtra")]
		public void GetExtra()
		{
			var request = GetRequest();
			request.AddProperty("One", new { Name = "John" });
			Assert.Equal(request.Properties["One"], new { Name = "John" });
			Assert.Equal(1, request.Depth);
		}

		[Fact(DisplayName = "Request_Dispose")]
		public void Dispose()
		{
			var request = GetRequest();
			Assert.Single(request.Properties);
			request.Dispose();
			Assert.Empty(request.Properties);
		}

		[Fact(DisplayName = "Request_Serialize")]
		public void Serialize()
		{
			var request = GetRequest();
			var str = JsonConvert.SerializeObject(request);
			var r = JsonConvert.DeserializeObject<Request>(str);
			Assert.Equal(request.Depth, r.Depth);
		}
	}
}

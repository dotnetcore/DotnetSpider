using Xunit;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;

namespace DotnetSpider.Core.Test
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

		[Fact]
		public void Request()
		{
			var request = GetRequest();
			Assert.Single(request.Extras);
			Assert.Equal(request.Extras["Test"], "Forever");
		}
		[Fact]
		public void PutExtra()
		{
			var request = GetRequest();
			request.PutExtra(null, null);
			request.PutExtra("", null);
			request.PutExtra("", "");
			request.PutExtra("", "");
			request.PutExtra("One", "One");
			request.PutExtra("One", "One");
			Assert.Equal(3, request.Extras.Count);
			Assert.Equal(request.Extras["One"], "One");
			Assert.Equal(request.Extras[""], "");
		}

		[Fact]
		public void GetExtra()
		{
			var request = GetRequest();
			request.PutExtra("One", new { Name = "John" });
			Assert.Equal(request.Extras["One"], new { Name = "John" });
			Assert.Equal(1, request.Depth);
		}

		[Fact]
		public void Dispose()
		{
			var request = GetRequest();
			Assert.Single(request.Extras);
			request.Dispose();
			Assert.Empty(request.Extras);
		}


		[Fact]
		public void Clone()
		{
			var request = GetRequest();
			var clone = (Request)request.Clone();
			Assert.Equal(request.Extras.Count, clone.Extras.Count);
			Assert.Equal(request.Depth, clone.Depth);
			Assert.Equal(request.Extras["Test"], clone.Extras["Test"]);
			Assert.Equal(request.Url, clone.Url);
			Assert.Equal(request.Method, clone.Method);
			Assert.Equal(request.Priority, clone.Priority);
		}

		[Fact]
		public void Serialize()
		{
			var request = GetRequest();
			var str = JsonConvert.SerializeObject(request);
			var r = JsonConvert.DeserializeObject<Request>(str);
			Assert.Equal(request.Depth, r.Depth);
		}
	}
}
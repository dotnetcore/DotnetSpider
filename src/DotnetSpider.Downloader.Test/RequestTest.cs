using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using Xunit;

namespace DotnetSpider.Downloader.Test
{
	public class RequestTest
	{
		public static Request GetRequest()
		{
			var extras = new Dictionary<string, dynamic> { { "Test", "Forever" } };
			var request = new Request("http://www.taobao.com", extras)
			{
				Method = HttpMethod.Get
			};
			return request;
		}

		[Fact(DisplayName = "RequestCreate")]
		public void Create()
		{
			var request = GetRequest();
			Assert.Single(request.Properties);
			Assert.Equal(request.Properties["Test"], "Forever");
		}

		[Fact(DisplayName = "RequestAddProperty")]
		public void AddProperty()
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

		[Fact(DisplayName = "RequestGetProperties")]
		public void GetProperties()
		{
			var request = GetRequest();
			request.AddProperty("One", new { Name = "John" });
			Assert.Equal(request.Properties["One"], new { Name = "John" });
		}

		[Fact(DisplayName = "RequestDispose")]
		public void Dispose()
		{
			var request = GetRequest();
			Assert.Single(request.Properties);
			request.Dispose();
			Assert.Empty(request.Properties);
		}

		[Fact(DisplayName = "RequestSerialize")]
		public void Serialize()
		{
			var request = GetRequest();
			var str = JsonConvert.SerializeObject(request);
			var r = JsonConvert.DeserializeObject<Request>(str);
		}
		
		[Fact(DisplayName = "RequestEqual")]
		public void Equal()
		{
			var r1=new Request("http://www.baidu.com");
			var r2=new Request("http://www.baidu.com");
			Assert.True(r1.Equals(r2));
			
			var r3=new Request("http://www.baidu.com");
			var r4=new Request("http://www.baidu.com", null);
			Assert.True(r3.Equals(r4));
			
			var r5=new Request("http://www.baidu.com");
			var r6=new Request("http://www.baidu.com", new Dictionary<string, dynamic>());
			Assert.True(r5.Equals(r6));
			
			var r7=new Request("http://www.baidu.com");
			var r8=new Request("http://www.baidu.com")
			{
				Headers = null
			};
			Assert.True(r7.Equals(r8));
		}
	}
}

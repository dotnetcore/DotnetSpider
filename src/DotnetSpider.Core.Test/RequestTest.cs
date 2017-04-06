using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DotnetSpider.Core.Test
{
	[TestClass]
	public class RequestTest
	{
		public static Request GetRequest()
		{
			var extras = new Dictionary<string, dynamic> { { "Test", "Forever" } };
			var request = new Request("http://www.taobao.com", extras)
			{
				Method = "get",
				Priority = 1
			};
			return request;
		}

		[TestMethod]
		public void Request()
		{
			var request = GetRequest();
			Assert.AreEqual(request.Extras.Count, 1);
			Assert.AreEqual(request.Extras["Test"], "Forever");
		}
		[TestMethod]
		public void PutExtra()
		{
			var request = GetRequest();
			request.PutExtra(null, null);
			request.PutExtra("", null);
			request.PutExtra("", "");
			request.PutExtra("", "");
			request.PutExtra("One", "One");
			request.PutExtra("One", "One");
			Assert.AreEqual(request.Extras.Count, 3);
			Assert.AreEqual(request.Extras["One"], "One");
			Assert.AreEqual(request.Extras[""], "");
		}

		[TestMethod]
		public void GetExtra()
		{
			var request = GetRequest();
			request.PutExtra("One", new { Name = "John" });
			Assert.AreEqual(request.Extras["One"], new { Name = "John" });
			Assert.AreEqual(request.Depth, 1);
		}

		[TestMethod]
		public void Dispose()
		{
			var request = GetRequest();
			Assert.AreEqual(request.Extras.Count, 1);
			request.Dispose();
			Assert.AreEqual(request.Extras.Count, 0);
		}


		[TestMethod]
		public void Clone()
		{
			var request = GetRequest();
			var clone = (Request)request.Clone();
			Assert.AreEqual(request.Extras.Count, clone.Extras.Count);
			Assert.AreEqual(request.Depth, clone.Depth);
			Assert.AreEqual(request.Extras["Test"], clone.Extras["Test"]);
			Assert.AreEqual(request.Url, clone.Url);
			Assert.AreEqual(request.Method, clone.Method);
			Assert.AreEqual(request.Priority, clone.Priority);
		}

		[TestMethod]
		public void Serialize()
		{
			var request = GetRequest();
			var str = JsonConvert.SerializeObject(request);
			var r = JsonConvert.DeserializeObject<Request>(str);
			Assert.AreEqual(request.Depth, r.Depth);
		}
	}
}
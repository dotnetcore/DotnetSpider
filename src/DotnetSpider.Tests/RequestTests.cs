using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DotnetSpider.Extensions;
using DotnetSpider.Http;
using DotnetSpider.Infrastructure;
using Xunit;
using ObjectId = MongoDB.Bson.ObjectId;

namespace DotnetSpider.Tests
{
	public class RequestTests
	{
		[Fact]
		public void StringClone()
		{
			var a = "hello";
			var b = a;
			b = "hi";

			Assert.True(a != "hi");
			Assert.True(a == "hello");
		}

		[Fact]
		public async Task SerializeAndDeserialize1()
		{
			var request = new Request("http://www.baidu.com")
			{
				Method = "PUT",
				Agent = "Agent",
				Downloader = Const.Downloader.HttpClient,
				Timestamp = 1000,
				PPPoERegex = "PPPoERegex"
			};
			request.Headers.UserAgent = ("UserAgent");
			request.Headers.Accept = ("Accept");
			var list = new List<byte>();
			for (int i = 0; i < 2000; ++i)
			{
				list.Add(byte.MinValue);
			}

			request.Content = (new ByteArrayContent(list.ToArray()));

			var bytes = request.Serialize();
			var r1 = await bytes.DeserializeAsync<Request>();
			Assert.Equal("PUT", r1.Method);
			Assert.Equal("Accept", r1.Headers.Accept);
			// Assert.Equal("Agent", r1.Agent);
			Assert.Equal(Const.Downloader.HttpClient, r1.Downloader);
			Assert.Equal("UserAgent", r1.Headers.UserAgent);
			Assert.Equal(1000, r1.Timestamp);
			Assert.Equal("PPPoERegex", r1.PPPoERegex);
			Assert.Equal(2000, ((ByteArrayContent)r1.Content as ByteArrayContent).Bytes.Length);
		}

		[Fact]
		public async Task SerializeAndDeserialize2()
		{
			var request = new Request("http://www.baidu.com")
			{
				Method = "PUT",
				Agent = "Agent",
				Downloader = Const.Downloader.HttpClient,
				Timestamp = 1000,
				PPPoERegex = "PPPoERegex"
			};
			request.Headers.UserAgent = ("UserAgent");
			request.Headers.Accept = ("Accept");

			request.Content = (new StringContent("{}", Encoding.UTF8, "application/json"));

			var bytes = request.Serialize();
			var r1 = await bytes.DeserializeAsync<Request>();
			Assert.Equal("PUT", r1.Method);
			Assert.Equal("UserAgent", r1.Headers.UserAgent);
			Assert.Equal("Accept", r1.Headers.Accept);
			// Assert.Equal("Agent", r1.Agent);
			Assert.Equal(Const.Downloader.HttpClient, r1.Downloader);

			Assert.Equal(1000, r1.Timestamp);
			Assert.Equal("PPPoERegex", r1.PPPoERegex);
			var content = (StringContent)r1.Content;
			Assert.Equal("{}", content.Content);
			Assert.Equal("UTF-8", content.EncodingName);
			Assert.Equal("application/json", content.MediaType);
		}


		[Fact]
		public void SerializeAndDeserialize3()
		{
			var hashAlgorithm = new MurmurHashAlgorithmService();
			var requestHasher = new RequestHasher(hashAlgorithm);
			var ownerId = ObjectId.GenerateNewId().ToString();
			var r1 = new Request("http://www.a.com") {Owner = ownerId};
			var h1 = requestHasher.ComputeHash(r1);

			var r2 = new Request("http://www.a.com") {Owner = ownerId};
			var h2 = requestHasher.ComputeHash(r2);
			Assert.Equal(h1, h2);
		}

		[Fact]
		public void DeepClone1()
		{
			var request = new Request("http://www.baidu.com")
			{
				Method = "PUT",
				Agent = "Agent",
				Downloader = Const.Downloader.HttpClient,
				Timestamp = 1000,
				PPPoERegex = "PPPoERegex"
			};
			request.Headers.UserAgent = ("UserAgent");
			request.Headers.Accept = ("Accept");
			var list = new List<byte>();
			for (int i = 0; i < 2000; ++i)
			{
				list.Add(byte.MinValue);
			}

			request.Content = (new ByteArrayContent(list.ToArray()));


			var r1 = request.Clone();
			Assert.Equal("PUT", r1.Method);

			// Assert.Equal("Agent", r1.Agent);
			Assert.Equal(Const.Downloader.HttpClient, r1.Downloader);
			Assert.Equal("UserAgent", r1.Headers.UserAgent);
			Assert.Equal("Accept", r1.Headers.Accept);
			Assert.Equal(1000, r1.Timestamp);
			Assert.Equal("PPPoERegex", r1.PPPoERegex);
			Assert.Equal(2000, ((ByteArrayContent)r1.Content).Bytes.Length);
		}

		[Fact]
		public void DeepClone2()
		{
			var request = new Request("http://www.baidu.com")
			{
				Method = "PUT",
				Agent = "Agent",
				Downloader = Const.Downloader.HttpClient,
				Timestamp = 1000,
				PPPoERegex = "PPPoERegex"
			};
			request.Headers.UserAgent = ("UserAgent");
			request.Headers.Accept = ("Accept");

			request.Content = (new StringContent("{}", Encoding.UTF8, "application/json"));

			var r1 = request.Clone();
			Assert.Equal("PUT", r1.Method);
			Assert.Equal("UserAgent", r1.Headers.UserAgent);
			Assert.Equal("Accept", r1.Headers.Accept);
			// Assert.Equal("Agent", r1.Agent);
			Assert.Equal(Const.Downloader.HttpClient, r1.Downloader);

			Assert.Equal(1000, r1.Timestamp);
			Assert.Equal("PPPoERegex", r1.PPPoERegex);
			var content = (StringContent)r1.Content;
			Assert.Equal("{}", content.Content);
			Assert.Equal("UTF-8", content.EncodingName);
			Assert.Equal("application/json", content.MediaType);
		}
	}
}

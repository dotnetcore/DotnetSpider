using System.Collections.Generic;
using System.Threading.Tasks;
using DotnetSpider.Downloader;
using DotnetSpider.Extensions;
using DotnetSpider.Http;
using Xunit;

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
				Accept = "Accept",
				Agent = "Agent",
				AutoRedirect = true,
				Downloader = DownloaderNames.HttpClient,
				UserAgent = "UserAgent",
				Timestamp = 1000,
				PPPoERegex = "PPPoERegex"
			};
			var list = new List<byte>();
			for (int i = 0; i < 2000; ++i)
			{
				list.Add(byte.MinValue);
			}

			request.SetContent(new ByteArrayContent(list.ToArray()));

			var bytes = request.Serialize();
			var r1 = await bytes.DeserializeAsync<Request>();
			Assert.Equal("PUT", r1.Method);
			Assert.Equal("Accept", r1.Accept);
			// Assert.Equal("Agent", r1.Agent);
			Assert.True(r1.AutoRedirect);
			Assert.Equal(DownloaderNames.HttpClient, r1.Downloader);
			Assert.Equal("UserAgent", r1.UserAgent);
			Assert.Equal(1000, r1.Timestamp);
			Assert.Equal("PPPoERegex", r1.PPPoERegex);
			Assert.Equal(2000, ((ByteArrayContent)r1.GetContentObject()).Bytes.Length);
		}

		[Fact]
		public async Task SerializeAndDeserialize2()
		{
			var request = new Request("http://www.baidu.com")
			{
				Method = "PUT",
				Accept = "Accept",
				Agent = "Agent",
				AutoRedirect = true,
				Downloader = DownloaderNames.HttpClient,
				UserAgent = "UserAgent",
				Timestamp = 1000,
				PPPoERegex = "PPPoERegex"
			};


			request.SetContent(new StringContent("{}", "application/json"));

			var bytes = request.Serialize();
			var r1 = await bytes.DeserializeAsync<Request>();
			Assert.Equal("PUT", r1.Method);
			Assert.Equal("Accept", r1.Accept);
			// Assert.Equal("Agent", r1.Agent);
			Assert.True(r1.AutoRedirect);
			Assert.Equal(DownloaderNames.HttpClient, r1.Downloader);
			Assert.Equal("UserAgent", r1.UserAgent);
			Assert.Equal(1000, r1.Timestamp);
			Assert.Equal("PPPoERegex", r1.PPPoERegex);
			var content = (StringContent)r1.GetContentObject();
			Assert.Equal("{}", content.Content);
			Assert.Equal("UTF-8", content.EncodingName);
			Assert.Equal("application/json", content.MediaType);
		}


		[Fact]
		public void DeepClone1()
		{
			var request = new Request("http://www.baidu.com")
			{
				Method = "PUT",
				Accept = "Accept",
				Agent = "Agent",
				AutoRedirect = true,
				Downloader = DownloaderNames.HttpClient,
				UserAgent = "UserAgent",
				Timestamp = 1000,
				PPPoERegex = "PPPoERegex"
			};
			var list = new List<byte>();
			for (int i = 0; i < 2000; ++i)
			{
				list.Add(byte.MinValue);
			}

			request.SetContent(new ByteArrayContent(list.ToArray()));


			var r1 = request.Clone();
			Assert.Equal("PUT", r1.Method);
			Assert.Equal("Accept", r1.Accept);
			// Assert.Equal("Agent", r1.Agent);
			Assert.True(r1.AutoRedirect);
			Assert.Equal(DownloaderNames.HttpClient, r1.Downloader);
			Assert.Equal("UserAgent", r1.UserAgent);
			Assert.Equal(1000, r1.Timestamp);
			Assert.Equal("PPPoERegex", r1.PPPoERegex);
			Assert.Equal(2000, ((ByteArrayContent)r1.GetContentObject()).Bytes.Length);
		}

		[Fact]
		public void DeepClone2()
		{
			var request = new Request("http://www.baidu.com")
			{
				Method = "PUT",
				Accept = "Accept",
				Agent = "Agent",
				AutoRedirect = true,
				Downloader = DownloaderNames.HttpClient,
				UserAgent = "UserAgent",
				Timestamp = 1000,
				PPPoERegex = "PPPoERegex"
			};


			request.SetContent(new StringContent("{}", "application/json"));

			var r1 = request.Clone();
			Assert.Equal("PUT", r1.Method);
			Assert.Equal("Accept", r1.Accept);
			// Assert.Equal("Agent", r1.Agent);
			Assert.True(r1.AutoRedirect);
			Assert.Equal(DownloaderNames.HttpClient, r1.Downloader);
			Assert.Equal("UserAgent", r1.UserAgent);
			Assert.Equal(1000, r1.Timestamp);
			Assert.Equal("PPPoERegex", r1.PPPoERegex);
			var content = (StringContent)r1.GetContentObject();
			Assert.Equal("{}", content.Content);
			Assert.Equal("UTF-8", content.EncodingName);
			Assert.Equal("application/json", content.MediaType);
		}
	}
}

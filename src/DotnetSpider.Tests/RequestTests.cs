using System.Collections.Generic;
using DotnetSpider.Agent;
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
        public void SerializeAndDeserialize1()
        {
            var request = new Request("http://www.baidu.com")
            {
                Method = "PUT",
                Accept = "Accept",
                Agent = "Agent",
                AutoRedirect = true,
                DownloaderType = DownloaderTypeNames.HttpClient,
                UserAgent = "UserAgent",
                Timestamp = 1000,
                RedialRegExp = "RedialRegExp"
            };
            var list = new List<byte>();
            for (int i = 0; i < 2000; ++i)
            {
                list.Add(byte.MinValue);
            }

            request.SetContent(new ByteArrayContent(list.ToArray()));

            var bytes = request.ToBytes();
            var r1 = bytes.ToRequest();
            Assert.Equal("PUT", r1.Method);
            Assert.Equal("Accept", r1.Accept);
            // Assert.Equal("Agent", r1.Agent);
            Assert.True(r1.AutoRedirect);
            Assert.Equal(DownloaderTypeNames.HttpClient, r1.DownloaderType);
            Assert.Equal("UserAgent", r1.UserAgent);
            Assert.Equal(1000, r1.Timestamp);
            Assert.Equal("RedialRegExp", r1.RedialRegExp);
            Assert.Equal(2000, ((ByteArrayContent) r1.GetContentObject()).Bytes.Length);
        }

        [Fact]
        public void SerializeAndDeserialize2()
        {
            var request = new Request("http://www.baidu.com")
            {
                Method = "PUT",
                Accept = "Accept",
                Agent = "Agent",
                AutoRedirect = true,
                DownloaderType = DownloaderTypeNames.HttpClient,
                UserAgent = "UserAgent",
                Timestamp = 1000,
                RedialRegExp = "RedialRegExp"
            };


            request.SetContent(new StringContent("{}", "application/json"));

            var bytes = request.ToBytes();
            var r1 = bytes.ToRequest();
            Assert.Equal("PUT", r1.Method);
            Assert.Equal("Accept", r1.Accept);
            // Assert.Equal("Agent", r1.Agent);
            Assert.True(r1.AutoRedirect);
            Assert.Equal(DownloaderTypeNames.HttpClient, r1.DownloaderType);
            Assert.Equal("UserAgent", r1.UserAgent);
            Assert.Equal(1000, r1.Timestamp);
            Assert.Equal("RedialRegExp", r1.RedialRegExp);
            var content = (StringContent) r1.GetContentObject();
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
                DownloaderType = DownloaderTypeNames.HttpClient,
                UserAgent = "UserAgent",
                Timestamp = 1000,
                RedialRegExp = "RedialRegExp"
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
            Assert.Equal(DownloaderTypeNames.HttpClient, r1.DownloaderType);
            Assert.Equal("UserAgent", r1.UserAgent);
            Assert.Equal(1000, r1.Timestamp);
            Assert.Equal("RedialRegExp", r1.RedialRegExp);
            Assert.Equal(2000, ((ByteArrayContent) r1.GetContentObject()).Bytes.Length);
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
                DownloaderType = DownloaderTypeNames.HttpClient,
                UserAgent = "UserAgent",
                Timestamp = 1000,
                RedialRegExp = "RedialRegExp"
            };


            request.SetContent(new StringContent("{}", "application/json"));

            var r1 = request.Clone();
            Assert.Equal("PUT", r1.Method);
            Assert.Equal("Accept", r1.Accept);
            // Assert.Equal("Agent", r1.Agent);
            Assert.True(r1.AutoRedirect);
            Assert.Equal(DownloaderTypeNames.HttpClient, r1.DownloaderType);
            Assert.Equal("UserAgent", r1.UserAgent);
            Assert.Equal(1000, r1.Timestamp);
            Assert.Equal("RedialRegExp", r1.RedialRegExp);
            var content = (StringContent) r1.GetContentObject();
            Assert.Equal("{}", content.Content);
            Assert.Equal("UTF-8", content.EncodingName);
            Assert.Equal("application/json", content.MediaType);
        }
    }
}

﻿using DotnetSpider.Core.Processor;
using System.Linq;
using Xunit;
using System.Net.Http;

namespace DotnetSpider.Core.Test.Processor
{
	
	public class TargetUrlTests
	{
		public class CnblogsProcessor1 : BasePageProcessor
		{
			public CnblogsProcessor1()
			{
				AddTargetUrlExtractor(".//div[@class='pager']", "/sitehome/p/\\d+", "^http://www\\.cnblogs\\.com/$");
			}

			protected override void Handle(Page page)
			{
				page.ResultItems.AddOrUpdateResultItem("test", true);
			}
		}

		[Fact]
		public void UrlVerifyAndExtract1()
		{
			HttpClient client = new HttpClient();
			var html = client.GetStringAsync("http://www.cnblogs.com").Result;

			Page page = new Page(new Request("http://www.cnblogs.com/", null), null);
			page.Content = html;

			CnblogsProcessor1 processor = new CnblogsProcessor1();
			processor.Site = new Site();
			processor.Process(page);

			Assert.True(page.ResultItems.GetResultItem("test"));
			Assert.Equal(12, page.TargetRequests.Count);
			Assert.Equal("http://www.cnblogs.com/", page.TargetRequests.ElementAt(11).Url.ToString());
		}

		public class CnblogsProcessor2 : BasePageProcessor
		{
			public CnblogsProcessor2()
			{
				AddTargetUrlExtractor(".//div[@class='pager']", "/sitehome/p/\\d+", "^http://www\\.cnblogs\\.com/$");
			}

			protected override void Handle(Page page)
			{
				page.ResultItems.AddOrUpdateResultItem("test", true);
			}
		}

		[Fact]
		public void UrlVerifyAndExtract2()
		{
			HttpClient client = new HttpClient();
			var html = client.GetStringAsync("http://www.cnblogs.com").Result;

			Page page = new Page(new Request("http://www.cnblogs.com/", null), null);
			page.Content = html;

			CnblogsProcessor2 processor = new CnblogsProcessor2();
			processor.Site = new Site();
			processor.Process(page);

			Assert.True(page.ResultItems.GetResultItem("test"));
			Assert.Equal(12, page.TargetRequests.Count);
			Assert.Equal("http://www.cnblogs.com/", page.TargetRequests.ElementAt(11).Url.ToString());
		}

		public class CnblogsProcessor3 : BasePageProcessor
		{
			public CnblogsProcessor3()
			{
				AddTargetUrlExtractor(".", "/sitehome/p/\\d+");
				Site = new Site();
			}

			protected override void Handle(Page page)
			{
				page.ResultItems.AddOrUpdateResultItem("test", true);
			}
		}

		[Fact]
        public void ProcessorFilterDefaultRequest()
        {
            Env.ProcessorFilterDefaultRequest = false;

            HttpClient client = new HttpClient();
			var html = client.GetStringAsync("http://www.cnblogs.com").Result;

			Page page = new Page(new Request("http://www.cnblogs.com/", null), null);
			page.Content = html;

			CnblogsProcessor3 processor = new CnblogsProcessor3();
			processor.Process(page);

			Assert.True(page.ResultItems.GetResultItem("test"));
			Assert.Equal(11, page.TargetRequests.Count);
		}

		public class CnblogsProcessor4 : BasePageProcessor
		{
			public CnblogsProcessor4()
			{
				AddTargetUrlExtractor(".", "/sitehome/p/\\d+");
			}

			protected override void Handle(Page page)
			{
				page.ResultItems.AddOrUpdateResultItem("test", true);
			}
		}

		[Fact]
		public void UrlVerifyAndExtract4()
		{
			HttpClient client = new HttpClient();
			var html = client.GetStringAsync("http://www.cnblogs.com/sitehome/p/2/").Result;

			Page page = new Page(new Request("http://www.cnblogs.com/sitehome/p/2/", null), null);
			page.Content = html;

			CnblogsProcessor4 processor = new CnblogsProcessor4();
			processor.Site = new Site();
			processor.Process(page);

			Assert.True(page.ResultItems.GetResultItem("test"));
			Assert.Equal(12, page.TargetRequests.Count);
			Assert.Equal("http://www.cnblogs.com/sitehome/p/2/", page.TargetRequests.ElementAt(0).Url.ToString());
		}
	}
}

using DotnetSpider.Core.Scheduler;
using System;
using Xunit;

namespace DotnetSpider.Core.Test
{

	public partial class SpiderTest
	{
		[Fact]
		public void IdentityLengthLimit()
		{
			try
			{
				Spider.Create(new Site { EncodingName = "UTF-8", SleepTime = 1000 },
					"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
					new QueueDuplicateRemovedScheduler(),
					new TestPageProcessor());
			}
			catch (Exception exception)
			{
				Assert.Equal($"Length of Identity should less than {Env.IdentityMaxLength}.", exception.Message);
				return;
			}

			throw new Exception("TEST FAILED.");
		}


		[Fact]
		public void RemoveOutboundLinksSetting()
		{
			try
			{
				var spider = Spider.Create(new Site { RemoveOutboundLinks = true },
						"1111",
						new QueueDuplicateRemovedScheduler(),
						new TestPageProcessor());
				spider.Run();
			}
			catch (Exception exception)
			{
				Assert.Equal($"When you want remove outbound links, the domains should not be null or empty.", exception.Message);
				return;
			}

			throw new Exception("TEST FAILED.");
		}
	}
}

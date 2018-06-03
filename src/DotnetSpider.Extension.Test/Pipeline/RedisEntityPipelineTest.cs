using DotnetSpider.Core;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extension.Processor;
using MessagePack;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DotnetSpider.Extension.Test.Pipeline
{
	public class RedisEntityPipelineTest
	{
		private string Html = @"
<div>
	<div class='int'>100</div>
	<div class='bool1'>true</div>
	<div class='bool2'>0</div>
	<div class='bigint'>200</div>
	<div class='string'>abcd</div>
	<div class='datetime'>2018-06-03</div>
	<div class='float'>1.1</div>
	<div class='double'>2.2</div>
	<div class='decimal'>0.8</div>
</div>
";

		private string connectString = "127.0.0.1:6379,serviceName=Scheduler.NET,keepAlive=8,allowAdmin=True,connectTimeout=10000,password=,abortConnect=True,connectRetry=20";

		[Fact(DisplayName = "RedisPipelineInsert")]
		public void Insert()
		{
			var processor = new EntityProcessor<N>(new ModelExtractor());
			var spider = new DefaultSpider();
			var page = CreatePage();
			processor.Process(page, spider);
			var guid = Guid.NewGuid().ToString("N");
			RedisEntityPipeline pipeline = new RedisEntityPipeline(guid, connectString);

			pipeline.Process(new[] { page.ResultItems }, spider);

			var connection = ConnectionMultiplexer.Connect(connectString);
			var db = connection.GetDatabase(0);
			var json = db.ListRightPop(guid);
			var item = LZ4MessagePackSerializer.Typeless.Deserialize(json) as Dictionary<string, object>;

			Assert.Equal("100", item["Int"]);
			Assert.Equal("2018-06-03", item["DateTime"]);
		}

		private Page CreatePage()
		{
			var request = new Request("http://dotnetspoder.me");
			var site = new Site();
			request.Site = site;
			var page = new Page(request);
			page.Content = Html;
			return page;
		}

		private class N
		{
			[Field(Expression = ".//div[@class='int']")]
			public int Int { get; set; }

			[Field(Expression = ".//div[@class='bool1']")]
			public bool Bool1 { get; set; }

			[Field(Expression = ".//div[@class='bool2']")]
			public bool Bool2 { get; set; }

			[Field(Expression = ".//div[@class='bigint']")]
			public long BigInt { get; set; }

			[Field(Expression = ".//div[@class='string']")]
			public string String { get; set; }

			[Field(Expression = ".//div[@class='datetime']")]
			public DateTime DateTime { get; set; }

			[Field(Expression = ".//div[@class='float']")]
			public float Float { get; set; }

			[Field(Expression = ".//div[@class='double']")]
			public double Double { get; set; }

			[Field(Expression = ".//div[@class='decimal']")]
			public decimal Decimal { get; set; }
		}
	}
}

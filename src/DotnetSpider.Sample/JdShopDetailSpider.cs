using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.ORM;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extension.Scheduler;
using DotnetSpider.Extension.Infrastructure;

namespace DotnetSpider.Sample
{
	public class JdShopDetailSpider : EntitySpiderBuilder
	{
		public JdShopDetailSpider() : base("JdShopDetailSpider", Batch.Now)
		{
		}

		protected override EntitySpider GetEntitySpider()
		{
			var context = new EntitySpider(new Site())
			{
				CachedSize = 1,
				ThreadNum = 8,
				Scheduler = new RedisScheduler("127.0.0.1:6379,serviceName=Scheduler.NET,keepAlive=8,allowAdmin=True,connectTimeout=10000,password=6GS9F2QTkP36GggE0c3XwVwI,abortConnect=True,connectRetry=20"),
				Downloader = new HttpClientDownloader
				{
					DownloadCompleteHandlers = new IDownloadCompleteHandler[]
					{
						new SubContentHandler
						{
							Start = "json(",
							End = ");",
							StartOffset = 5,
							EndOffset = 0
						}
					}
				},
				PrepareStartUrls = new PrepareStartUrls[]
				{
					new BaseDbPrepareStartUrls()
					{
						Source = DataSource.MySql,
						ConnectString = "Database='test';Data Source= localhost;User ID=root;Password=1qazZAQ!;Port=3306",
						QueryString = $"SELECT * FROM jd.sku_v2_{DateTimeUtils.RunIdOfMonday} WHERE shopname is null or shopid is null order by sku",
						Columns = new [] {new DataColumn { Name = "sku"} },
						FormateStrings = new List<string> { "http://chat1.jd.com/api/checkChat?my=list&pidList={0}&callback=json" }
					}
				}
			};
			context.AddPipeline(new MySqlEntityPipeline("Database='taobao';Data Source=localhost ;User ID=root;Password=1qazZAQ!;Port=4306"));
			context.AddEntityType(typeof(ProductUpdater));
			return context;
		}

		[Table("jd", "sku_v2", TableSuffix.Monday, Primary = "Sku", UpdateColumns = new[] { "ShopId" })]
		[EntitySelector(Expression = "$.[*]", Type = SelectorType.JsonPath)]
		public class ProductUpdater : SpiderEntity
		{
			[PropertyDefine(Expression = "$.pid", Type = SelectorType.JsonPath, Length = 25)]
			public string Sku { get; set; }

			[PropertyDefine(Expression = "$.seller", Type = SelectorType.JsonPath, Length = 100)]
			public string ShopName { get; set; }

			[PropertyDefine(Expression = "$.shopId", Type = SelectorType.JsonPath, Length = 25)]
			public string ShopId { get; set; }
		}
	}
}

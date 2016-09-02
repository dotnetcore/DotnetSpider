using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Core.Common;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.ORM;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extension.Scheduler;

namespace DotnetSpider.Sample
{
	public class JdShopDetailSpider : EntitySpiderBuilder
	{
		protected override EntitySpider GetEntitySpider()
		{
			var context = new EntitySpider(new Site())
			{
				TaskGroup = "JD SKU Weekly",
				Identity = "JD Shop details " + DateTimeUtils.RunIdOfMonday,
				CachedSize = 1,
				ThreadNum = 8,
				Scheduler = new RedisScheduler
				{
					Host = "redis",
					Port = 6379,
					Password = "#frAiI^MtFxh3Ks&swrnVyzAtRTq%w"
				},
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
			context.AddEntityPipeline(new MySqlEntityPipeline
			{
				ConnectString = "Database='taobao';Data Source=localhost ;User ID=root;Password=1qazZAQ!;Port=4306",
				Mode = PipelineMode.Update
			});
			context.AddEntityType(typeof(ProductUpdater), new TargetUrlExtractor
			{
				Region = new Selector { Type = SelectorType.XPath, Expression = "//*[@id=\"J_bottomPage\"]" },
				Patterns = new List<string> { @"&page=[0-9]+&" }
			});
			return context;
		}

		[Schema("jd", "sku_v2", TableSuffix.Monday)]
		[EntitySelector(Expression = "$.[*]", Type = SelectorType.JsonPath)]
		[Indexes(Primary = "sku")]
		public class ProductUpdater : ISpiderEntity
		{
			[StoredAs("sku", DataType.String, 25)]
			[PropertySelector(Expression = "$.pid", Type = SelectorType.JsonPath)]
			public string Sku { get; set; }

			[StoredAs("shopname", DataType.String, 100)]
			[PropertySelector(Expression = "$.seller", Type = SelectorType.JsonPath)]
			public string ShopName { get; set; }

			[StoredAs("shopid", DataType.String, 25)]
			[PropertySelector(Expression = "$.shopId", Type = SelectorType.JsonPath)]
			public string ShopId { get; set; }
		}
	}
}

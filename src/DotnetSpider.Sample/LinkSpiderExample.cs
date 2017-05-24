//using System;
//using System.Collections.Generic;
//using DotnetSpider.Core.Common;
//using DotnetSpider.Core;
//using DotnetSpider.Core.Downloader;
//using DotnetSpider.Extension;
//using DotnetSpider.Extension.Configuration;
//using DotnetSpider.Extension.Model;
//using DotnetSpider.Extension.Model.Attribute;
//using DotnetSpider.Extension.Model.Formatter;
//using DotnetSpider.Extension.ORM;
//using Newtonsoft.Json.Linq;
//using DownloadValidation = DotnetSpider.Extension.Configuration.DownloadValidation;

//namespace DotnetSpider.Test.Example
//{
//	public class LinkSpiderExample : ILinkEntitySpider
//	{
//		public EntitySpiderBuilder GetBuilder()
//		{
//			return new EntitySpiderBuilder(new EntitySpider
//			{
//				SpiderName = "JD sku/store test " + DateTime.Now.ToString("yyyy-MM-dd"),
//				CachedSize = 1,
//				ThreadNum = 1,
//				StartUrls = new Dictionary<string, Dictionary<string, object>>
//				{
//					{"http://list.jd.com/list.html?cat=9987,653,655&page=1&ext=57050::1943^^&go=0&JL=6_0_0",new Dictionary<string, object> { { "name", "手机"}, { "cat3", "655" } } },
//					{"http://list.jd.com/list.html?cat=9987,653,655&page=2&ext=57050::1943^^&go=0&JL=6_0_0",new Dictionary<string, object> { { "name", "手机"}, { "cat3", "655" } } },
//					{"http://list.jd.com/list.html?cat=9987,653,655&page=3&ext=57050::1943^^&go=0&JL=6_0_0",new Dictionary<string, object> { { "name", "手机"}, { "cat3", "655" } } },
//				},
//				PrepareStartUrls = new List<PrepareStartUrls> {
//					new CyclePrepareStartUrls {
//						From=0,
//						To=10000,
//						FormateString="http://list.jd.com/list.html?cat=9987,653,655&page=1&ext=57050::{0}^^&go=0&JL=6_0_0"
//					}
//				},
//				Scheduler = new RedisScheduler
//				{
//					Host = "redis",
//					Password = "#frAiI^MtFxh3Ks&swrnVyzAtRTq%w",
//					Port = 6379
//				},
//				Pipelines =new List<Extension.Configuration.Pipeline> { new MysqlPipeline
//				{
//					ConnectString = ""
//				} },
//				Downloader = new HttpDownloader()
//				{
//				}
//			}, typeof(Product));
//		}

//		public Dictionary<string, EntitySpiderBuilder> GetNextSpiders()
//		{
//			EntitySpiderBuilder nextBuilder1 = new EntitySpiderBuilder(new EntitySpider());
//			return new Dictionary<string, EntitySpiderBuilder>
//			{
//				{ EntitySpiderBuilder.GetEntityName(typeof(Product)),new EntitySpiderBuilder(new EntitySpider
//				{
//					Site = new Site
//					{
//						Headers = new Dictionary<string, string>
//						{
//							{ "token","" }
//						}
//					},
//					PrepareStartUrls = new List<PrepareStartUrls>
//					{
//						new LinkSpiderPrepareStartUrls
//						{
//							Columns = new List<BaseDbPrepareStartUrls.Column> { new BaseDbPrepareStartUrls.Column { Name = "token"} },
//							FormateStrings = new List<string>
//							{
//								"http://asdfasdfasd"
//							},
//						}
//					}
//				})}
//			};
//		}

//		[Schema("test", "sku", TableSuffix.Today)]
//		[TypeExtractBy(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]", Multi = true)]
//		[Indexes(Index = new[] { "category" }, Unique = new[] { "category,sku", "sku" })]
//		public class Product : SpiderEntity
//		{
//			[PropertyExtractBy(Expression = "./@data-sku")]
//			public string Sku { get; set; }

//			[PropertyExtractBy(Expression = "set-cookie",Type= ExtractType.Enviroment)]
//			public string Cookie { get; set; }
//		}
//	}
//}

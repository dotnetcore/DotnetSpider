using System;
using System.Collections.Generic;
using Java2Dotnet.Spider.Common;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Core.Downloader;
using Java2Dotnet.Spider.Extension;
using Java2Dotnet.Spider.Extension.Configuration;
using Java2Dotnet.Spider.Extension.Model;
using Java2Dotnet.Spider.Extension.Model.Attribute;
using Java2Dotnet.Spider.Extension.Model.Formatter;
using Java2Dotnet.Spider.Extension.ORM;
using Newtonsoft.Json.Linq;
using DownloadValidation = Java2Dotnet.Spider.Extension.Configuration.DownloadValidation;

namespace Java2Dotnet.Spider.Test.Example
{
	public class LinkSpiderExample : ILinkSpiderContext
	{
		public SpiderContextBuilder GetBuilder()
		{
			return new SpiderContextBuilder(new SpiderContext
			{
				SpiderName = "JD sku/store test " + DateTime.Now.ToString("yyyy-MM-dd"),
				CachedSize = 1,
				ThreadNum = 1,
				StartUrls = new Dictionary<string, Dictionary<string, object>>
				{
					{"http://list.jd.com/list.html?cat=9987,653,655&page=1&ext=57050::1943^^&go=0&JL=6_0_0",new Dictionary<string, object> { { "name", "手机"}, { "cat3", "655" } } },
					{"http://list.jd.com/list.html?cat=9987,653,655&page=2&ext=57050::1943^^&go=0&JL=6_0_0",new Dictionary<string, object> { { "name", "手机"}, { "cat3", "655" } } },
					{"http://list.jd.com/list.html?cat=9987,653,655&page=3&ext=57050::1943^^&go=0&JL=6_0_0",new Dictionary<string, object> { { "name", "手机"}, { "cat3", "655" } } },
				},
				PrepareStartUrls = new List<PrepareStartUrls> {
					new CyclePrepareStartUrls {
						From=0,
						To=10000,
						FormateString="http://list.jd.com/list.html?cat=9987,653,655&page=1&ext=57050::{0}^^&go=0&JL=6_0_0"
					}
				},
				Scheduler = new RedisScheduler
				{
					Host = "redis",
					Password = "#frAiI^MtFxh3Ks&swrnVyzAtRTq%w",
					Port = 6379
				},
				Pipeline = new MysqlPipeline
				{
					ConnectString = ""
				},
				Downloader = new HttpDownloader()
				{
				}
			}, typeof(Product));
		}

		public Dictionary<string, SpiderContextBuilder> GetNextSpiders()
		{
			SpiderContextBuilder nextBuilder1 = new SpiderContextBuilder(new SpiderContext());
			return new Dictionary<string, SpiderContextBuilder>
			{
				{ SpiderContextBuilder.GetEntityName(typeof(Product)),new SpiderContextBuilder(new SpiderContext
				{
					Site = new Site
					{
						Headers = new Dictionary<string, string>
						{
							{ "token","" }
						}
					},
					PrepareStartUrls = new List<PrepareStartUrls>
					{
						new LinkSpiderPrepareStartUrls
						{
							Columns = new List<DbPrepareStartUrls.Column> { new DbPrepareStartUrls.Column { Name = "token"} },
							FormateStrings = new List<string>
							{
								"http://asdfasdfasd"
							},
						}
					}
				})}
			};
		}

		[Schema("test", "sku", TableSuffix.Today)]
		[TypeExtractBy(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]", Multi = true)]
		[Indexes(Index = new[] { "category" }, Unique = new[] { "category,sku", "sku" })]
		public class Product : ISpiderEntity
		{
			[PropertyExtractBy(Expression = "./@data-sku")]
			public string Sku { get; set; }

			[PropertyExtractBy(Expression = "set-cookie",Type= ExtractType.Enviroment)]
			public string Cookie { get; set; }
		}
	}
}

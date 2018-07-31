using DotnetSpider.Common;
using DotnetSpider.Core;
using DotnetSpider.Downloader;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extraction;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Extraction.Model.Attribute;
using DotnetSpider.Sample.docs;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace DotnetSpider.Sample
{
	class Program
	{
		static void Main(string[] args)
		{
#if NETCOREAPP
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#else
			ThreadPool.SetMinThreads(256, 256);
#endif

			DataHandlerSpider.Run();
		}

		/// <summary>
		/// <c>MyTest</c> is a method in the <c>Program</c>
		/// </summary>
		private static void MyTest()
		{

		}

		public class DataHandlerSpider
		{
			public static void Run()
			{
				Spider spider = new Spider();
				spider.Run();
			}
			public class Spider : EntitySpider
			{
				public Spider() : base(new Site
				{
					Headers = new Dictionary<string, string>
				{
					{ "Accept","application/json, text/javascript, */*; q=0.01" },
					{ "Referer", "http://www.bidepharmatech.com/cn/product/detail/BD8243.html" },
					{ "Cache-Control", "no-cache" },
					{ "Connection", "keep-alive" },
					{ "Content-Type", "application/json; charset=UTF-8" },
					{ "User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.79 Safari/537.36" }
				}
				})
				{
				}

				protected override void OnInit(params string[] arguments)
				{
					Identity = Identity ?? "BD SKU SAMPLE";
					Request res = new Request();
					res.Content = "{\"webType\":0,\"bdNo\":\"BD8243\",\"uuid\":\"ef-7f0e83-9261-4959\"}";
					res.Url = "http://www.bidepharmatech.com/web/product/queryStock";
					res.Method = System.Net.Http.HttpMethod.Post;
					AddPipeline(new MySqlEntityPipeline());
					AddStartRequest(res);
					AddEntityType<BDItem>();
					Downloader = new FakeDownloader(File.ReadAllText("TextFile1.txt"));
				}
			}
			[TableInfo("mysql", "bd_sku_20180726")]
			[EntitySelector(Expression = "$.feedback", Type = SelectorType.JsonPath)]
			private class BDItem : BaseEntity
			{
				[FieldSelector(Expression = "$.priceId", Type = SelectorType.JsonPath)]
				public string price { get; set; }

				[FieldSelector(Expression = "$.china1US", Type = SelectorType.JsonPath)]
				public string cat { get; set; }


			}
		}

	}
}

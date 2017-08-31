using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.ORM;
using DotnetSpider.Extension.Pipeline;

namespace DotnetSpider.Sample
{
	public class JdShopDetailSpider2 : EntitySpider
	{
		public JdShopDetailSpider2() : base("JdShopDetailSpider2", new Site
		{
			UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36",
			Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8",
			Headers = new Dictionary<string, string>
				{
					{ "Accept-Encoding"  ,"gzip, deflate, sdch" },
					{ "Upgrade-Insecure-Requests"  ,"1" },
					{ "Accept-Language"  ,"en,en-US;q=0.8" },
					{ "Cache-Control" , "ax-age=0" },
				}
		})
		{
		}

		protected override void MyInit(params string[] arguments)
		{
			Site.AddStartUrl("http://chat1.jd.com/api/checkChat?my=list&pidList=3355984&callback=json");
			Site.AddStartUrl("http://chat1.jd.com/api/checkChat?my=list&pidList=3682523&callback=json");
			var downloader = new HttpClientDownloader();
			//downloader.AddAfterDownloadCompleteHandler(new SubContentHandler
			//{
			//	Start = "json(",
			//	End = ");",
			//	StartOffset = 5,
			//	EndOffset = 2
			//});

			AddPipeline(new MySqlEntityPipeline("Database='mysql';Data Source=localhost ;User ID=root;Password=1qazZAQ;Port=3306"));
			AddEntityType(typeof(ProductUpdater));
		}

		[Table("jd", "shop", TableSuffix.Monday, Primary = "pid")]
		[EntitySelector(Expression = "$.[*]", Type = SelectorType.JsonPath)]
		class ProductUpdater : SpiderEntity
		{
			[PropertyDefine(Expression = "$.pid", Type = SelectorType.JsonPath, Length = 25)]
			public string pid { get; set; }

			[PropertyDefine(Expression = "$.seller", Type = SelectorType.JsonPath, Length = 100)]
			public string seller { get; set; }

			[PropertyDefine(Expression = "$.shopId", Type = SelectorType.JsonPath, Length = 25)]
			public string shopId { get; set; }

			[PropertyDefine(Expression = "$.venderid", Type = SelectorType.JsonPath, Length = 25)]
			public string venderid { get; set; }
		}
	}
}

using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;

using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure.Database;
using System;

namespace DotnetSpider.Sample.docs
{
	public class DbStartUrlsBuilderSpider : EntitySpider
	{
		protected override void MyInit(params string[] arguments)
		{
			Downloader.AddAfterDownloadCompleteHandler(new CutoutHandler("json(", ");", 5, 0));
			AddPipeline(new ConsoleEntityPipeline());
			AddStartUrlBuilder(new DbStartUrlsBuilder(Database.MySql, Env.DataConnectionString,
				$"SELECT * FROM test.jd_sku", new[] { "sku" },
				"http://chat1.jd.com/api/checkChat?my=list&pidList={0}&callback=json"));

			AddEntityType<Item>();
		}

		[TableInfo("test", "jd_sku", TableNamePostfix.Monday, Uniques = new[] { "Sku" }, UpdateColumns = new[] { "ShopId" })]
		[EntitySelector(Expression = "$.[*]", Type = SelectorType.JsonPath)]
		class Item
		{
			[Field(Expression = "$.pid", Type = SelectorType.JsonPath, Length = 25)]
			public string Sku { get; set; }

			[Field(Expression = "$.shopId", Type = SelectorType.JsonPath)]
			public int ShopId { get; set; }
		}
	}
}

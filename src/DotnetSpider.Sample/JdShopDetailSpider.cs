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

namespace DotnetSpider.Sample
{
	public class JdShopDetailSpider : EntitySpider
	{
		public JdShopDetailSpider() : base("JdShopDetailSpider", new Site())
		{
		}

		protected override void MyInit(params string[] arguments)
		{
			Identity = Identity ?? Guid.NewGuid().ToString();
			Downloader.AddAfterDownloadCompleteHandler(new SubContentHandler("json(", ");", 5, 0));

		    AddStartUrlBuilder(new DbStartUrlBuilder(Database.MySql,
		        Env.DataConnectionStringSettings.ConnectionString,
		        $"SELECT * FROM jd_sku_{DateTimeUtils.MondayOfCurrentWeek.ToString("yyyy_MM_dd")} WHERE ShopName is null or ShopId is null or ShopId = 0 order by sku", new[] { "sku" },
		        "http://chat1.jd.com/api/checkChat?my=list&pidList={0}&callback=json"));
            AddPipeline(new MySqlEntityPipeline(Env.DataConnectionStringSettings.ConnectionString));
            AddEntityType<ProductUpdater>();
		}

		[EntityTable("DotnetSpider", "jd_sku", EntityTable.Monday, Uniques = new[] { "Sku" }, UpdateColumns = new[] { "ShopId" })]
		[EntitySelector(Expression = "$.[*]", Type = SelectorType.JsonPath)]
		class ProductUpdater : SpiderEntity
		{
			[PropertyDefine(Expression = "$.pid", Type = SelectorType.JsonPath, Length = 25)]
			public string Sku { get; set; }

			[PropertyDefine(Expression = "$.shopId", Type = SelectorType.JsonPath)]
			public int ShopId { get; set; }
		}
	}
}

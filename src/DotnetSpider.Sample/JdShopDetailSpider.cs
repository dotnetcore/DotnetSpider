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
			Downloader.AddAfterDownloadCompleteHandler(new CutoutHandler("json(", ");", 5, 0));

			AddStartUrlBuilder(new DbStartUrlsBuilder(Database.MySql,
				"Database='mysql';Data Source=localhost;User ID=root;Password=;Port=3306;SslMode=None;",
				$"SELECT * FROM test.jd_sku_{DateTimeUtil.Monday.ToString("yyyy_MM_dd")} WHERE ShopName is null or ShopId is null or ShopId = 0 order by sku", new[] { "sku" },
				"http://chat1.jd.com/api/checkChat?my=list&pidList={0}&callback=json"));
			AddPipeline(new MySqlEntityPipeline("Database='mysql';Data Source=localhost;User ID=root;Password=;Port=3306;SslMode=None;"));
			AddEntityType<ProductUpdater>();
		}

		[TableInfo("test", "jd_sku", TableNamePostfix.Monday, Uniques = new[] { "Sku" }, UpdateColumns = new[] { "ShopId" })]
		[EntitySelector(Expression = "$.[*]", Type = SelectorType.JsonPath)]
		class ProductUpdater
		{
			[Field(Expression = "$.pid", Type = SelectorType.JsonPath, Length = 25)]
			public string Sku { get; set; }

			[Field(Expression = "$.shopId", Type = SelectorType.JsonPath)]
			public int ShopId { get; set; }
		}
	}
}

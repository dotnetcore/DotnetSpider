using DotnetSpider.Common;
using DotnetSpider.Core;
using DotnetSpider.Downloader.AfterDownloadCompleteHandlers;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extraction;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Extraction.Model.Attribute;

namespace DotnetSpider.Sample.docs
{
	public class DbRequestBuilderSpider : EntitySpider
	{
		protected override void OnInit(params string[] arguments)
		{
			Downloader.AddAfterDownloadCompleteHandler(new CutoutHandler("json(", ");", 5, 0));
			AddPipeline(new ConsoleEntityPipeline());
			AddRequestBuilder(new DbRequestBuilder(Database.MySql, Env.DataConnectionString,
				$"SELECT * FROM test.jd_sku", new[] { "sku" },
				"http://chat1.jd.com/api/checkChat?my=list&pidList={0}&callback=json"));

			AddEntityType<Item>();
		}

		[TableInfo("test", "jd_sku", TableNamePostfix.Monday, Uniques = new[] { "Sku" }, UpdateColumns = new[] { "ShopId" })]
		[EntitySelector(Expression = "$.[*]", Type = SelectorType.JsonPath)]
		class Item
		{
			[FieldSelector(Expression = "$.pid", Type = SelectorType.JsonPath, Length = 25)]
			public string Sku { get; set; }

			[FieldSelector(Expression = "$.shopId", Type = SelectorType.JsonPath)]
			public int ShopId { get; set; }
		}
	}
}

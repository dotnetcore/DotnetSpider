using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extension.ORM;
using Xunit;

namespace DotnetSpider.Test.Pipeline
{
	public class MysqlEntityPipelineTest
	{
		[Fact]
		public void TestInsert()
		{
			MySqlEntityPipeline pipeline = new MySqlEntityPipeline("Database='mysql';Data Source=127.0.0.1;User ID=root;Password=1qazZAQ!;Port=3306");
			pipeline.InitiEntity(new Schema("test", "sku", TableSuffix.Today), EntitySpider.PaserEntityMetaData(typeof(Product).GetTypeInfo()));
		}

		[Schema("test", "sku", TableSuffix.Today)]
		[TypeExtractBy(Expression = "//li[@class='gl-item']/div[contains(@class,'j-sku-item')]", Multi = true)]
		[Indexes(Index = new[] { "category" }, Unique = new[] { "category,sku", "sku" })]
		public class Product : ISpiderEntity
		{
			[StoredAs("category", DataType.String, 20)]
			[PropertyExtractBy(Expression = "name", Type = ExtractType.Enviroment)]
			public string CategoryName { get; set; }

			[StoredAs("url", DataType.Text)]
			[PropertyExtractBy(Expression = "./div[1]/a/@href")]
			public string Url { get; set; }

			[PropertyExtractBy(Expression = "Now", Type = ExtractType.Enviroment)]
			[StoredAs("cdate", DataType.Time)]
			public DateTime CDate { get; set; }
		}
	}
}

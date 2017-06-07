using System;
using DotnetSpider.Core;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Infrastructure;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.ORM;
using DotnetSpider.Extension.Pipeline;
using Newtonsoft.Json;

namespace DotnetSpider.Sample
{
	public class JdCategorySpider : EntitySpiderBuilder
	{
		public JdCategorySpider() : base("京东类目 Daliy Tracking", Batch.Now)
		{
		}

		[Table("jd", "jd_category")]
		[EntitySelector(Expression = ".//div[@class='items']//a")]
		public class Category : SpiderEntity
		{
			[PropertyDefine(Expression = ".")]
			public string CategoryName { get; set; }

			[PropertyDefine(Expression = "./@href")]
			public string Url { get; set; }
		}

		protected override EntitySpider GetEntitySpider()
		{
			var entitySpider = new EntitySpider(new Site());

			entitySpider.AddStartUrl("http://www.jd.com/allSort.aspx");
			entitySpider.AddEntityType(typeof(Category));
			entitySpider.AddPipeline(new MySqlEntityPipeline("Database='mysql';Data Source=localhost;User ID=root;Password=1qazZAQ!;Port=3306"));

			var t = JsonConvert.SerializeObject(entitySpider);
			return entitySpider;
		}
	}
}

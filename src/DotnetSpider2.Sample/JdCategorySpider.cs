using DotnetSpider.Core;
using DotnetSpider.Core.Common;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Model.Formatter;
using DotnetSpider.Extension.ORM;
using DotnetSpider.Extension.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetSpider.Sample
{
	public class JdCategorySpider : EntitySpiderBuilder
	{
		[Schema("jd", "jd_category")]
		[EntitySelector(Expression = ".//div[@class='items']//a")]
		public class Category : ISpiderEntity
		{
			[StoredAs("name", DataType.String, 50)]
			[PropertySelector(Expression = ".")]
			public string CategoryName { get; set; }

			[StoredAs("url", DataType.Text)]
			[PropertySelector(Expression = "./@href")]
			public string Url { get; set; }
		}

		protected override EntitySpider GetEntitySpider()
		{
			var entitySpider = new EntitySpider(new Site())
			{
				Identity = "JdCategory Daliy Tracking " + DateTimeUtils.Day1OfThisWeek.ToString("yyyy-MM-dd")
			};

			entitySpider.AddStartUrl("http://www.jd.com/allSort.aspx");
			entitySpider.AddEntityType(typeof(Category));
			entitySpider.AddEntityPipeline(new MySqlEntityPipeline("Database='mysql';Data Source=localhost;User ID=root;Password=1qazZAQ!;Port=3306"));
			return entitySpider;
		}
	}
}

using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.ORM;
using DotnetSpider.Extension.Pipeline;

namespace DotnetSpider.Sample
{
	public class JdCategorySpider : EntitySpider
	{
		public JdCategorySpider() : base("京东类目 Daliy Tracking")
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


		protected override void MyInit(params string[] arguments)
		{
			AddStartUrl("http://www.jd.com/allSort.aspx");
			AddEntityType(typeof(Category));
			AddPipeline(new MySqlEntityPipeline("Database='mysql';Data Source=localhost;User ID=root;Password=1qazZAQ!;Port=3306"));
		}
	}
}

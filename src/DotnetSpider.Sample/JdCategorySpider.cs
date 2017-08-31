using DotnetSpider.Core;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.ORM;
using DotnetSpider.Extension.Pipeline;

namespace DotnetSpider.Sample
{
	public class JdCategorySpider : EntitySpider
	{
		public JdCategorySpider() : base("京东类目_Daliy_Tracking", new Site())
		{
		}

		[Table("jd", "jd_category")]
		[EntitySelector(Expression = ".//div[@class='items']//a")]
		class Category : SpiderEntity
		{
			[PropertyDefine(Expression = ".", Length = 100)]
			public string CategoryName { get; set; }

			[PropertyDefine(Expression = "./@href")]
			public string Url { get; set; }
		}


		protected override void MyInit(params string[] arguments)
		{
			AddStartUrl("http://www.jd.com/allSort.aspx");
			AddEntityType(typeof(Category));
			AddPipeline(new MySqlEntityPipeline("Database='mysql';Data Source=localhost;User ID=root;Password=1qazZAQ;Port=3306"));
		}
	}
}

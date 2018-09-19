using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Extraction.Model.Attribute;
using System;

namespace DotnetSpider.Sample.docs
{
	public class MultiEntityModelSpider
	{
		public static void Run()
		{
			CnblogsSpider spider = new CnblogsSpider();
			spider.Run();
		}

		private class CnblogsSpider : EntitySpider
		{
			protected override void OnInit(params string[] arguments)
			{
				Identity = ("cnblogs_" + DateTime.Now.ToString("yyyy_MM_dd_HHmmss"));
				AddRequests("http://www.cnblogs.com");
				AddRequests("https://www.cnblogs.com/news/");
				AddPipeline(new ConsoleEntityPipeline());
				AddEntityType<News>();
				AddEntityType<BlogSumary>();
			}

			[Target(Patterns = new[] { "^http://www\\.cnblogs\\.com/news/$", "www\\.cnblogs\\.com/news/\\d+" })]
			[Entity(Expression = "//div[@class='post_item']")]
			class News : BaseEntity
			{
				[Field(Expression = ".//a[@class='titlelnk']")]
				public string Name { get; set; }

				[Field(Expression = ".//div[@class='post_item_foot']/a[1]")]
				public string Author { get; set; }

				[Field(Expression = ".//div[@class='post_item_foot']/text()")]
				public string PublishTime { get; set; }

				[Field(Expression = ".//a[@class='titlelnk']/@href")]
				public string Url { get; set; }
			}

			[Target(Patterns = new[] { "^http://www\\.cnblogs\\.com/$", "http://www\\.cnblogs\\.com/sitehome/p/\\d+" })]
			[Entity(Expression = "//div[@class='post_item']")]
			class BlogSumary : BaseEntity
			{
				[Field(Expression = ".//a[@class='titlelnk']")]
				public string Name { get; set; }

				[Field(Expression = ".//div[@class='post_item_foot']/a[1]")]
				public string Author { get; set; }

				[Field(Expression = ".//div[@class='post_item_foot']/text()")]
				public string PublishTime { get; set; }

				[Field(Expression = ".//a[@class='titlelnk']/@href")]
				public string Url { get; set; }
			}
		}
	}
}

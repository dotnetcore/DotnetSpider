using DotnetSpider.Extension;
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
				AddStartUrl("http://www.cnblogs.com");
				AddStartUrl("https://www.cnblogs.com/news/");
				AddPipeline(new ConsoleEntityPipeline());
				AddEntityType<News>();
				AddEntityType<BlogSumary>();
			}

			[TargetRequestSelector(Patterns = new[] { "^http://www\\.cnblogs\\.com/news/$", "www\\.cnblogs\\.com/news/\\d+" })]
			[EntitySelector(Expression = "//div[@class='post_item']")]
			class News : BaseEntity
			{
				[FieldSelector(Expression = ".//a[@class='titlelnk']")]
				public string Name { get; set; }

				[FieldSelector(Expression = ".//div[@class='post_item_foot']/a[1]")]
				public string Author { get; set; }

				[FieldSelector(Expression = ".//div[@class='post_item_foot']/text()")]
				public string PublishTime { get; set; }

				[FieldSelector(Expression = ".//a[@class='titlelnk']/@href")]
				public string Url { get; set; }
			}

			[TargetRequestSelector(Patterns = new[] { "^http://www\\.cnblogs\\.com/$", "http://www\\.cnblogs\\.com/sitehome/p/\\d+" })]
			[EntitySelector(Expression = "//div[@class='post_item']")]
			class BlogSumary : BaseEntity
			{
				[FieldSelector(Expression = ".//a[@class='titlelnk']")]
				public string Name { get; set; }

				[FieldSelector(Expression = ".//div[@class='post_item_foot']/a[1]")]
				public string Author { get; set; }

				[FieldSelector(Expression = ".//div[@class='post_item_foot']/text()")]
				public string PublishTime { get; set; }

				[FieldSelector(Expression = ".//a[@class='titlelnk']/@href")]
				public string Url { get; set; }
			}
		}
	}
}

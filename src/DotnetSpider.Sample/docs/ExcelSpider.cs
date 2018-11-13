using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extraction;
using DotnetSpider.Extraction.Model.Attribute;

namespace DotnetSpider.Sample.docs
{
	public class ExcelSpider : EntitySpider
	{
		protected override void OnInit(params string[] arguments)
		{
			//AddRequest("www.cnblogs.com", new Dictionary<string, object> {
			//	{"Accept-Language","zh-CN,zh;q=0.8" },
			//	{"UserAgent","Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36" }
			//});
			AddRequests("https://www.cnblogs.com/");
			AddEntityType<BlogSearchEntry>();
			AddPipeline(new ConsoleEntityPipeline());
		}

		[Entity(Expression = "#post_list div.post_item", Type = SelectorType.Css)]
		public class BlogSearchEntry: BaseEntity
		{
			[Column]
			[Field(Expression = "div.post_item_body", Type = SelectorType.Css)]
			[ExcelFormatter("[html].innerText('h3 a',0).md5()")]
			public string Hash { get; set; }


			[Column]
			[Field(Expression = "div.post_item_body", Type = SelectorType.Css)]
			[ExcelFormatter("[html].innerText('h3 a',0)")]
			public string Title { get; set; }

			[Column]
			[Field(Expression = "div.post_item_body", Type = SelectorType.Css)]
			[ExcelFormatter("[html].OuterHtml('h3 a',0).attr('href')")]
			public string Href { get; set; }


			[Column]
			[Field(Expression = "div.post_item_body", Type = SelectorType.Css)]
			[ExcelFormatter("[html].OuterHtml('h3 a',0).hasattr('href')")]
			public bool HasHref { get; set; }

			[Column]
			[Field(Expression = "div.post_item_body", Type = SelectorType.Css)]
			[ExcelFormatter("[html].OuterHtml('h3 a',0).hasclass('href')")]
			public bool HasClass { get; set; }

		}

	}
}

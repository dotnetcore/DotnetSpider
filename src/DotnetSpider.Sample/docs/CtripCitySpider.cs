using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extraction;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Extraction.Model.Attribute;
using System;
using System.Collections.Generic;

namespace DotnetSpider.Sample.docs
{
	public class CtripCitySpider : EntitySpider
	{
		protected override void OnInit(params string[] arguments)
		{
			AddHeaders("www.ctrip.com", new Dictionary<string, object> {
				{"Cache-Control","max-age=0" },
				{"Upgrade-Insecure-Requests","1" },
				{"Accept-Encoding","gzip, deflate, sdch" },
				{"Accept-Language","zh-CN,zh;q=0.8" },
				{"Accept","text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8" },
				{"UserAgent","Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36" }
			});
			AddRequests("http://www.ctrip.com/");
			AddEntityType<CtripCity>();
			AddPipeline(new ConsoleEntityPipeline());
		}

		[Schema("ctrip", "city")]
		[Entity(Expression = "//div[@class='city_item']//a")]
		class CtripCity : IBaseEntity
		{
			[Column]
			[Field(Expression = ".")]
			public string name { get; set; }

			[Column]
			[Field(Expression = "./@title")]
			public string title { get; set; }

			[Column]
			[Field(Expression = "./@data-id")]
			[Unique("CITYID_RUNID")]
			public string city_id { get; set; }

			[Column]
			[Unique("CITYID_RUNID")]
			[Field(Expression = "Today", Type = SelectorType.Enviroment)]
			public DateTime run_id { get; set; }
		}
	}
}

using System;
using DotnetSpider.Core;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.ORM;
using DotnetSpider.Extension.Pipeline;

namespace DotnetSpider.Sample
{
	public class ShanxizhaoshengSpider : EntitySpider
	{
		public ShanxizhaoshengSpider() : base("ShanxizhaoshengSpider")
		{
		}

		protected override void MyInit(params string[] arguments)
		{
			Identity = ("ShanxizhaoshengSpider " + DateTime.Now.ToString("yyyy-MM-dd HHmmss"));
			SetSite(new Site
			{
				EncodingName = "GB2312"
			});
			AddPipeline(new SqlServerEntityPipeline("Data Source=.\\SQLEXPRESS;Initial Catalog=master;Integrated Security=True"));
			AddStartUrl("http://www.sneac.com/pgjhcx/ypbkyxjg.jsp?a11709CountNo=2000");
			AddEntityType(typeof(Item));
		}

		[Table("abc", "shanxizhaosheng")]
		[EntitySelector(Expression = "/html/body/table[3]/tbody/tr/td/table/tbody/tr/td/table[2]/tbody/tr/td/table/tr/td/a")]
		public class Item : SpiderEntity
		{
			[PropertyDefine(Expression = ".")]
			public string School { get; set; }

			[PropertyDefine(Expression = "Now", Type = SelectorType.Enviroment)]
			public DateTime CDate { get; set; }
		}
	}
}

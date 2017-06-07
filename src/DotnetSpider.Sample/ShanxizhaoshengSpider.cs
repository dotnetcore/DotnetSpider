using System;
using DotnetSpider.Core;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.ORM;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extension.Infrastructure;

namespace DotnetSpider.Sample
{
	public class ShanxizhaoshengSpider : EntitySpiderBuilder
	{
		public ShanxizhaoshengSpider() : base("ShanxizhaoshengSpider", Batch.Now)
		{
		}

		protected override EntitySpider GetEntitySpider()
		{
			EntitySpider context = new EntitySpider(new Site());
			context.SetIdentity("ShanxizhaoshengSpider " + DateTime.Now.ToString("yyyy-MM-dd HHmmss"));
			context.SetSite(new Site
			{
				EncodingName = "GB2312"
			});
			context.AddPipeline(new SqlServerEntityPipeline("Data Source=.\\SQLEXPRESS;Initial Catalog=master;Integrated Security=True"));
			context.AddStartUrl("http://www.sneac.com/pgjhcx/ypbkyxjg.jsp?a11709CountNo=2000");
			context.AddEntityType(typeof(Item));

			return context;
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

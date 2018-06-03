using System;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Pipeline;

namespace DotnetSpider.Sample
{
	public class RegexTestEntitySpider : EntitySpider
	{
		public RegexTestEntitySpider() : base("RegexTest")
		{
		}


		protected override void MyInit(params string[] arguments)
		{
			Identity = ("cnblogs homepage " + DateTime.Now.ToString("yyyy-MM-dd HHmmss"));
			AddStartUrl("http://www.cnblogs.com");
			AddPipeline(new ConsoleEntityPipeline());
			AddEntityType<HomePage>();
		}

		class HomePage
		{
			//jQuery(".yk-rank div:1")
			[Field(Expression = "<a.*?т╟вс</a>", Type = SelectorType.Regex, Arguments = "1")]
			public string Category { get; set; }
		}
	}
}
using System;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Pipeline;

namespace DotnetSpider.Sample.docs
{
	public class RegexSelectorSpider : EntitySpider
	{
		protected override void MyInit(params string[] arguments)
		{
			AddStartUrl("http://www.cnblogs.com");
			AddPipeline(new ConsoleEntityPipeline());
			AddEntityType<HomePage>();
		}

		class HomePage
		{
			[Field(Expression = "<a.*?т╟вс</a>", Type = SelectorType.Regex, Arguments = "1")]
			public string Category { get; set; }
		}
	}
}
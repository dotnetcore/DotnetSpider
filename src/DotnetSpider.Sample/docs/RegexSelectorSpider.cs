using DotnetSpider.Extension;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extraction;
using DotnetSpider.Extraction.Model.Attribute;

namespace DotnetSpider.Sample.docs
{
	public class RegexSelectorSpider : EntitySpider
	{
		protected override void OnInit(params string[] arguments)
		{
			AddStartUrl("http://www.cnblogs.com");
			AddPipeline(new ConsoleEntityPipeline());
			AddEntityType<HomePage>();
		}

		class HomePage
		{
			[FieldSelector(Expression = "<a.*?园子</a>", Type = SelectorType.Regex, Arguments = "1")]
			public string Category { get; set; }
		}
	}
}
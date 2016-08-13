namespace DotnetSpider.Extension.Model.Attribute
{
	public class BaseSelector : Selector
	{
		public long Limit { get; set; } = long.MaxValue;
		public string Argument { get; set; }
	}
}

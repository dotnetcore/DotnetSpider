namespace DotnetSpider.Extension.Model
{
	public class Selector : System.Attribute
	{
		public ExtractType Type { get; set; }
		public string Expression { get; set; }
		public object Argument { get; set; }
	}
}

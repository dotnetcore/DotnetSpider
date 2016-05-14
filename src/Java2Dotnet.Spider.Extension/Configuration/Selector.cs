using Java2Dotnet.Spider.Extension.Model;

namespace Java2Dotnet.Spider.Extension.Configuration
{
	public class Selector
	{
		public ExtractType Type { get; set; }
		public string Expression { get; set; }
		public object Argument { get; set; }
	}
}

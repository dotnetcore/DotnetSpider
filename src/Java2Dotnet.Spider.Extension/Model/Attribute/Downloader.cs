namespace Java2Dotnet.Spider.Extension.Model.Attribute
{
	public class Downloader : System.Attribute
	{
		public Configuration.Downloader.Types Type { get; set; }
		public object Arguments { get; set; }
	}
}

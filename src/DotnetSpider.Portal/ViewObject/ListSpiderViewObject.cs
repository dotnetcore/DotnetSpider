namespace DotnetSpider.Portal.ViewObject
{
	public class ListSpiderViewObject
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Image { get; set; }
		public string Cron { get; set; }
		public string Type { get; set; }
		public string Environment { get; set; }
		public string Volume { get; set; }
		public bool Enabled { get; set; }
		public string CreationTime { get; set; }
		public string LastModificationTime { get; set; }
	}
}

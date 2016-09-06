using System;

namespace DotnetSpider.Portal.Models
{
	public class Source
	{
		public int Id { get; set; }
		public string GitUrl { get; set; }
		public string ProjectName { get; set; }
		public string Identity { get; set; }
		public string EntryProjectPath { get; set; }
		public DateTime CDate { get; set; }
	}
}

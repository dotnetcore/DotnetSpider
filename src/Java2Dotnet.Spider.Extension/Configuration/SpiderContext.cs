using System;
using System.Collections.Generic;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Extension.Configuration;
using Newtonsoft.Json.Linq;

namespace Java2Dotnet.Spider.Extension
{
	public class SpiderContext
	{
		// build it internal
		internal List<JObject> Entities { get; set; } = new List<JObject>();

		public string SpiderName { get; set; }
		public int ThreadNum { get; set; } = 1;
		public int Deep { get; set; } = int.MaxValue;
		public int EmptySleepTime { get; set; } = 15000;
		public int CachedSize { get; set; } = 1;
		public Configuration.Scheduler Scheduler { get; set; }
		public Configuration.Downloader Downloader { get; set; }
		public Site Site { get; set; }
		public bool NeedRedial { get; set; }
		public NetworkValidater NetworkValidater { get; set; }
		public Redialer Redialer { get; set; }
		public PrepareStartUrls PrepareStartUrls { get; set; }
		public Dictionary<string, Dictionary<string, object>> StartUrls { get; set; } = new Dictionary<string, Dictionary<string, object>>();
		public Configuration.Pipeline Pipeline { get; set; }
		public string Corporation { get; set; }
		public string ValidationReportTo { get; set; }
		public CustomizePage CustomizePage { get; set; }
		public CustomizeTargetUrls CustomizeTargetUrls { get; set; }
	}
}

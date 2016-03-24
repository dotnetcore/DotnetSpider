using System.Collections.Generic;
using Java2Dotnet.Spider.Core;
using Newtonsoft.Json.Linq;

namespace Java2Dotnet.Spider.Extension.Configuration
{
	public class SpiderContext
	{
		public string SpiderName { get; set; }
		public int ThreadNum { get; set; } = 1;
		public int Deep { get; set; } = int.MaxValue;
		public int EmptySleepTime { get; set; } = 15000;
		public int CachedSize { get; set; } = 1;
		public JObject Scheduler { get; set; }
		public JObject Downloader { get; set; }
		public Site Site { get; set; }
		public bool NeedRedial { get; set; }
		public NetworkValidater NetworkValidater { get; set; }
		public JObject Redialer { get; set; }
		public JObject PrepareStartUrls { get; set; }
		public Dictionary<string, Dictionary<string, object>> StartUrls { get; set; } = new Dictionary<string, Dictionary<string, object>>();
		public JObject Pipeline { get; set; }
		public List<JObject> Entities { get; set; } = new List<JObject>();
		public string Corporation { get; set; }
		public string ValidationReportTo { get; set; }
		public JObject CustomizePage { get; set; }
		public JObject CustomizeTargetUrls { get; set; }
	}
}

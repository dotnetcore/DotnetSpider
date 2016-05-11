using System.Collections.Generic;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Extension.Configuration;
using Newtonsoft.Json.Linq;

namespace Java2Dotnet.Spider.Extension
{
	public class SpiderContext
	{
		// build it internal
		public List<JObject> Entities { get; internal set; } = new List<JObject>();

		public string SpiderName { get; set; }
		public string UserId { get; set; }
		public string TaskGroup { get; set; }
		public int ThreadNum { get; set; } = 1;
		public int Deep { get; set; } = int.MaxValue;
		public int EmptySleepTime { get; set; } = 15000;
		public int CachedSize { get; set; } = 1;
		public Configuration.Scheduler Scheduler { get; set; }
		public Configuration.Downloader Downloader { get; set; }
		public Site Site { get; set; }
		public NetworkValidater NetworkValidater { get; set; }
		public Redialer Redialer { get; set; }
		public List<PrepareStartUrls> PrepareStartUrls { get; set; }
		public Dictionary<string, Dictionary<string, object>> StartUrls { get; set; } = new Dictionary<string, Dictionary<string, object>>();
		public Configuration.Pipeline Pipeline { get; set; }
		public List<PageHandler> PageHandlers { get; set; }
		public TargetUrlsHandler TargetUrlsHandler { get; set; }
		public List<EnviromentValue> EnviromentValues { get; set; }
		public Validations Validations { get; set; }
		public CookieTrapper GetCookie { get; set; }

		public ISpider ToDefaultSpider()
		{
			return new DefaultSpider("", new Site());
		}
	}

	public class LinkSpiderContext : SpiderContext
	{
		public Dictionary<string, SpiderContext> NextSpiderContexts { get; set; }
	}
}

using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetSpider.Extension.Downloader
{
	public abstract class BeforeDownloadHandler : Named, IBeforeDownloadHandler
	{
		public abstract void Handle(Request request, ISpider spider);
	}

	public class GeneratePostBodyHandler : BeforeDownloadHandler
	{
		public List<string> ArgumnetNames { get; set; }

		public override void Handle(Request request, ISpider spider)
		{
			List<string> arguments = new List<string>();
			foreach (var arg in ArgumnetNames)
			{
				var tmp = spider.Site.Arguments.ContainsKey(arg) ? spider.Site.Arguments[arg] : "";
				arguments.Add(tmp);
			}
			request.PostBody = string.Format(request.PostBody, arguments.Select(a => (object)a));
		}
	}
}

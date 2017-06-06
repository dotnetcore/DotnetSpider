using System.Collections.Generic;
using System.Linq;

namespace DotnetSpider.Core.Downloader
{
	public interface IBeforeDownloadHandler
	{
		void Handle(Request request, ISpider spider);
	}

	public abstract class BeforeDownloadHandler : Named, IBeforeDownloadHandler
	{
		public abstract void Handle(Request request, ISpider spider);
	}

	public class GeneratePostBodyHandler : BeforeDownloadHandler
	{
		public string[] ArgumnetNames { get; set; }

		public override void Handle(Request request, ISpider spider)
		{
			List<string> arguments = new List<string>();
			foreach (var arg in ArgumnetNames)
			{
				if (spider.Site.Arguments.ContainsKey(arg))
				{
					arguments.Add(spider.Site.Arguments[arg]);
				}
				else
				{
					arguments.Add(request.ExistExtra(arg) ? request.GetExtra(arg) : "");
				}
			}
			var args = arguments.Select(a => (object) a).ToArray();
			request.PostBody = string.Format(request.PostBody, args);
		}
	}
}

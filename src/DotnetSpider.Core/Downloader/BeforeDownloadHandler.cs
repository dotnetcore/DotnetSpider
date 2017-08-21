using System.Collections.Generic;
using System.Linq;

namespace DotnetSpider.Core.Downloader
{
	public abstract class BeforeDownloadHandler : Named, IBeforeDownloadHandler
	{
		public abstract bool Handle(ref Request request, ISpider spider);
	}
}

using DotnetSpider.Downloader;
using System.Collections.Generic;

namespace DotnetSpider.Core.Processor
{
	public interface IRequestExtractor
	{
		IEnumerable<Request> Extract(Page page);
	}
}

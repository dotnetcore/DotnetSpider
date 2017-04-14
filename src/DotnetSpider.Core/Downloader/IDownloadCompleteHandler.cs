using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Core.Downloader
{
	public interface IDownloadCompleteHandler
	{
		bool Handle(Page page, ISpider spider);
	}
}

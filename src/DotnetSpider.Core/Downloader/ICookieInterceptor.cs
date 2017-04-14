using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetSpider.Core.Downloader
{
	public interface ICookieInjector
	{
		void Inject(ISpider spider, bool stopSpider = true);
	}
}

using DotnetSpider.Downloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Core.Processor
{
	public interface IFilter
	{
		bool IsMatch(Request request);
	}
}

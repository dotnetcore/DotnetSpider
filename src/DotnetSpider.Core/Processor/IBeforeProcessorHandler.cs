using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotnetSpider.Core.Processor
{
	public interface IBeforeProcessorHandler
	{
		void Handle(ref Page page);
	}
}

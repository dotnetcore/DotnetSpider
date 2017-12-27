using DotnetSpider.Core;
using System.Collections.Generic;

namespace DotnetSpider.Extension.Pipeline
{
	public interface IEntityPipeline
	{
		int Process(string entity, IEnumerable<dynamic> datas, ISpider spider);
	}
}

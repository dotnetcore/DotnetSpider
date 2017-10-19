using System.Collections.Generic;

namespace DotnetSpider.Extension.Pipeline
{
	public interface IEntityPipeline
	{
		int Process(string name, List<dynamic> datas);
	}
}

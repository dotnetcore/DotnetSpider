using System.Collections.Generic;
using DotnetSpider.Extension.Model;

namespace DotnetSpider.Extension.Pipeline
{
	public interface IEntityPipeline
	{
		int Process(string name, List<dynamic> datas);
	}
}

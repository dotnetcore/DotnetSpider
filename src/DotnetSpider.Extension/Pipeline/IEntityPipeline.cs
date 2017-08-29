using System.Collections.Generic;
using DotnetSpider.Extension.Model;

namespace DotnetSpider.Extension.Pipeline
{
	public interface IEntityPipeline
	{
		void Process(string entityName, List<DataObject> datas);
	}
}

using DotnetSpider.Core;
using DotnetSpider.Extension.Model;
using System.Collections.Generic;

namespace DotnetSpider.Extension.Pipeline
{   
	/// <summary>
	/// 爬虫实体类对应的数据管道
	/// </summary>
	public interface IModelPipeline
	{
		/// <summary>
		/// 处理爬虫实体解析器解析到的实体数据结果
		/// </summary>
		/// <param name="identity">爬虫实体类的名称</param>
		/// <param name="datas">实体类数据</param>
		/// <param name="spider">爬虫</param>
		/// <returns>最终影响结果数量(如数据库影响行数)</returns>
		int Process(IModel model, IEnumerable<dynamic> datas, ISpider spider);
	}
}

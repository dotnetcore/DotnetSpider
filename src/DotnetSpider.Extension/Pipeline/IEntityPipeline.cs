using DotnetSpider.Core;
using System.Collections.Generic;

namespace DotnetSpider.Extension.Pipeline
{
	/// <summary>
	/// 爬虫实体类对应的数据管道
	/// </summary>
	public interface IEntityPipeline
	{
		/// <summary>
		/// 处理爬虫实体解析器解析到的实体数据结果
		/// </summary>
		/// <param name="name">爬虫实体类的名称</param>
		/// <param name="datas">实体类数据</param>
		/// <param name="spider">爬虫</param>
		/// <returns>最终影响结果数量(如数据库影响行数)</returns>
		int Process(string name, IEnumerable<dynamic> datas, ISpider spider);
	}
}

using System.Collections.Generic;

namespace DotnetSpider.Extraction.Model
{
	public interface IModelExtractor
	{
		/// <summary>
		/// 解析成实体对象
		/// </summary>
		/// <param name="selectable">可查询对象</param>
		/// <param name="model">解析模型</param>
		/// <returns>实体对象</returns>
		IList<dynamic> Extract(Selectable selectable, IModel model);
	}
}

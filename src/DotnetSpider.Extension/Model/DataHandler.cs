using System.Collections.Generic;
using DotnetSpider.Core;
using Newtonsoft.Json.Linq;

namespace DotnetSpider.Extension.Model
{
	public abstract class DataHandler
	{
		protected abstract JObject HandleDataOject(JObject data, Page page);

		public virtual List<JObject> Handle(List<JObject> datas, Page page)
		{
			if (datas == null || datas.Count == 0)
			{
				return datas;
			}

			List<JObject> results =new List<JObject>();
			foreach (var data in datas)
			{
				var tmp = HandleDataOject(data, page);
				if (tmp != null)
				{
					results.Add(tmp);
				}
			}
			return results;
		}
	}
}

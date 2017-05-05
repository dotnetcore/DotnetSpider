using System.Collections.Generic;
using DotnetSpider.Core;
using Newtonsoft.Json.Linq;

namespace DotnetSpider.Extension.Model
{
	public abstract class DataHandler
	{
		public virtual List<JObject> Handle(List<JObject> datas, Page page)
		{
			List<JObject> results = new List<JObject>();
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

		protected abstract JObject HandleDataOject(JObject data, Page page);
	}
}

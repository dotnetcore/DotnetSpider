using System.Collections.Generic;
using DotnetSpider.Core;

namespace DotnetSpider.Extension.Model
{
	public abstract class DataHandler
	{
		protected abstract DataObject HandleDataOject(DataObject data, Page page);

		public virtual List<DataObject> Handle(List<DataObject> datas, Page page)
		{
			if (datas == null || datas.Count == 0)
			{
				return datas;
			}

			List<DataObject> results =new List<DataObject>();
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

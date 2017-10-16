using System.Collections.Generic;
using DotnetSpider.Core;

namespace DotnetSpider.Extension.Model
{
	public interface IDataHandler
	{
	}

	public abstract class DataHandler<T> : IDataHandler
	{
		protected abstract T HandleDataOject(T data, Page page);

		public virtual List<T> Handle(List<T> datas, Page page)
		{
			if (datas == null || datas.Count == 0)
			{
				return datas;
			}

			List<T> results = new List<T>();
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

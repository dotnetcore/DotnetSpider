using System.Collections.Generic;
using System.Linq;
using DotnetSpider.Core;

namespace DotnetSpider.Extension.Model
{
	/// <summary>
	/// 对解析的结果进一步加工操作
	/// </summary>
	/// <typeparam name="T">数据结果的类型</typeparam>
	public interface IDataHandler<T>
	{
		/// <summary>
		/// 对Processor的结果进一步加工操作
		/// </summary>
		/// <param name="datas">数据结果</param>
		/// <param name="page">页面信息</param>
		/// <returns>加工后的数据结果</returns>
		IEnumerable<T> Handle(IEnumerable<T> datas, Page page);
	}

	/// <summary>
	/// 对解析的结果进一步加工操作
	/// </summary>
	/// <typeparam name="T">数据结果的类型</typeparam>
	public abstract class DataHandler<T> : IDataHandler<T>
	{
		/// <summary>
		/// 对Processor的结果进一步加工操作
		/// </summary>
		/// <param name="data">数据结果</param>
		/// <param name="page">页面信息</param>
		/// <returns>加工后的数据结果</returns>
		protected abstract T HandleDataOject(T data, Page page);

		/// <summary>
		/// 对Processor的结果进一步加工操作
		/// </summary>
		/// <param name="datas">数据结果</param>
		/// <param name="page">页面信息</param>
		/// <returns>加工后的数据结果</returns>
		public virtual IEnumerable<T> Handle(IEnumerable<T> datas, Page page)
		{
			if (datas == null || datas.Count() == 0)
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

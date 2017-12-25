using System.Collections.Concurrent;

namespace DotnetSpider.Core
{
	/// <summary>
	/// 存储页面解析的数据结果
	/// 此对象包含在页面对象中, 并传入数据管道被处理
	/// </summary>
	public class ResultItems : ConcurrentDictionary<string, dynamic>
	{
		/// <summary>
		/// 当前解析结果对应的目标链接信息
		/// </summary>
		public Request Request { get; set; }

		/// <summary>
		/// 通过键值取得数据结果
		/// </summary>
		/// <param name="key">键值</param>
		/// <returns>数据结果</returns>
		public dynamic GetResultItem(string key)
		{
			return TryGetValue(key, out dynamic result) ? result : null;
		}
	}
}
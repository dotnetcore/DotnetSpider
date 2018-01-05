using System.Collections.Concurrent;

namespace DotnetSpider.Core.Infrastructure
{
	/// <summary>
	/// ConcurrentDictionary 扩展
	/// </summary>
	public static class ConcurrentDictionaryExtensions
	{
		/// <summary>
		/// Adds a key/value pair if the key does not already exist, or updates a key/value pair in the ConcurrentDictionary by using the specified function if the key already exists.
		/// </summary>
		/// <typeparam name="TK"></typeparam>
		/// <typeparam name="TV"></typeparam>
		/// <param name="dictionary">ConcurrentDictionary</param>
		/// <param name="key">The key to be added or whose value should be updated</param>
		/// <param name="value">The value to be added/updated for an absent key</param>
		public static void AddOrUpdate<TK, TV>(this ConcurrentDictionary<TK, TV> dictionary, TK key, TV value)
		{
			dictionary.AddOrUpdate(key, value, (oldkey, oldvalue) => value);
		}
	}
}

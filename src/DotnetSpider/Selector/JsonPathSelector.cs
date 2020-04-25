using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DotnetSpider.Selector
{
	/// <summary>
	/// JsonPath selector.
	/// Used to extract content from JSON.
	/// </summary>
	public class JsonPathSelector : ISelector
	{
		private readonly string _jsonPath;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="jsonPath">JsonPath</param>
		public JsonPathSelector(string jsonPath)
		{
			_jsonPath = jsonPath;
		}

		/// <summary>
		/// 从JSON文本中查询单个结果
		/// 如果符合条件的结果有多个, 仅返回第一个
		/// </summary>
		/// <param name="text">需要查询的Json文本</param>
		/// <returns>查询结果</returns>
		public ISelectable Select(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return null;
			}

			if (!(JsonConvert.DeserializeObject(text) is JToken token))
			{
				return null;
			}

			var result = token.SelectToken(_jsonPath);
			return result == null ? null : new JsonSelectable(result);
		}

		/// <summary>
		/// 从JSON文本中查询所有结果
		/// </summary>
		/// <param name="text">需要查询的Json文本</param>
		/// <returns>查询结果</returns>
		public IEnumerable<ISelectable> SelectList(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return null;
			}

			var token = JsonConvert.DeserializeObject(text) as JToken;
			if (token == null)
			{
				return Enumerable.Empty<ISelectable>();
			}

			return token.SelectTokens(_jsonPath)
				.Select(x => new JsonSelectable(x));
		}
	}
}

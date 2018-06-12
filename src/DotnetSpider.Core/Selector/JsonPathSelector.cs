using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DotnetSpider.Core.Selector
{
	/// <summary>
	/// JsonPath selector. 
	/// Used to extract content from JSON. 
	/// </summary>
	public class JsonPathSelector : ISelector
	{
		private readonly string _jsonPath;

		public JsonPathSelector(string jsonPath)
		{
			_jsonPath = jsonPath;
		}

		public dynamic Select(dynamic text)
		{
			IList<dynamic> result = SelectList(text);
			if (result.Count > 0)
			{
				return result[0];
			}
			return null;
		}

		public IEnumerable<dynamic> SelectList(dynamic text)
		{
			if (text != null)
			{
				List<dynamic> list = new List<dynamic>();
				if (text is string)
				{
					if (JsonConvert.DeserializeObject(text) is JObject o)
					{
						var items = o.SelectTokens(_jsonPath).Select(t=>t.ToString()).ToList();
						if (items.Count > 0)
						{
							list.AddRange(items);
						}
					}
					else
					{
						JArray array = JsonConvert.DeserializeObject(text) as JArray;
						var items = array?.SelectTokens(_jsonPath).Select(t => t.ToString()).ToList();
						if (items != null && items.Count > 0)
						{
							list.AddRange(items);
						}
					}
				}
				else
				{
					dynamic realText = text is HtmlNode node ? JsonConvert.DeserializeObject<JObject>(node.InnerText) : text;

					if (realText is JObject o)
					{
						var items = o.SelectTokens(_jsonPath).Select(t => t.ToString()).ToList();
						if (items.Count > 0)
						{
							list.AddRange(items);
						}
					}
					else
					{
						JArray array = text as JArray;
						var items = array?.SelectTokens(_jsonPath).Select(t => t.ToString()).ToList();
						if (items != null && items.Count > 0)
						{
							list.AddRange(items);
						}
					}
				}
				return list;
			}
			else
			{
				return new List<dynamic>();
			}
		}
	}
}
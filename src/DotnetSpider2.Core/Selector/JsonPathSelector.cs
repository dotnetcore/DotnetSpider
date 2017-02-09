using System.Collections.Generic;
using System.Linq;
#if NET_CORE
using DotnetSpider.HtmlAgilityPack;
#else
using HtmlAgilityPack;
#endif
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
		private readonly string _jsonPathStr;

		public JsonPathSelector(string jsonPathStr)
		{
			_jsonPathStr = jsonPathStr;
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

		public List<dynamic> SelectList(dynamic text)
		{
			if (text != null)
			{
				List<dynamic> list = new List<dynamic>();
				if (text is string)
				{
					JObject o = JsonConvert.DeserializeObject(text) as JObject;

					if (o != null)
					{
						var items = o.SelectTokens(_jsonPathStr).Select(t=>t.ToString()).ToList();
						if (items.Count > 0)
						{
							list.AddRange(items);
						}
					}
					else
					{
						JArray array = JsonConvert.DeserializeObject(text) as JArray;
						var items = array?.SelectTokens(_jsonPathStr).Select(t => t.ToString()).ToList();
						if (items != null && items.Count > 0)
						{
							list.AddRange(items);
						}
					}
				}
				else
				{
					var node = text as HtmlNode;
					dynamic realText = node != null ? JsonConvert.DeserializeObject<JObject>(node.InnerText) : text;
					JObject o = realText as JObject;

					if (o != null)
					{
						var items = o.SelectTokens(_jsonPathStr).Select(t => t.ToString()).ToList();
						if (items.Count > 0)
						{
							list.AddRange(items);
						}
					}
					else
					{
						JArray array = text as JArray;
						var items = array?.SelectTokens(_jsonPathStr).Select(t => t.ToString()).ToList();
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
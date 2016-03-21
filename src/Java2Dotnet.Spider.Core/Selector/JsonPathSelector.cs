using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Java2Dotnet.Spider.Core.Selector
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
						var items = o.SelectTokens(_jsonPathStr).ToList();
						list.AddRange(items.Select(i => i.ToString()));
					}
					else
					{
						JArray a = JsonConvert.DeserializeObject(text) as JArray;
						var items = a.SelectTokens(_jsonPathStr).ToList();
						list.AddRange(items.Select(i => i.ToString()));
					}
				}
				else
				{
					JObject o = text as JObject;

					if (o != null)
					{
						var items = o.SelectTokens(_jsonPathStr).ToList();
						list.AddRange(items.Select(i => i.ToString()));
					}
					else
					{
						JArray a = text as JArray;
						var items = a.SelectTokens(_jsonPathStr).ToList();
						list.AddRange(items.Select(i => i.ToString()));
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
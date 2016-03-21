using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Java2Dotnet.Spider.Extension.Configuration
{
	public interface IJobject
	{
	}

	public static class JsonObjectExtension
	{
		public static JObject ToJObject(this IJobject jsonObj)
		{
			return JsonConvert.DeserializeObject(JsonConvert.SerializeObject(jsonObj)) as JObject;
		}
	}
}

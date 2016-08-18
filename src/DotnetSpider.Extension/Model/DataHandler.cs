using DotnetSpider.Core;
using Newtonsoft.Json.Linq;

namespace DotnetSpider.Extension.Model
{
	public abstract class DataHandler
	{
		public abstract void Handle(JObject data, Page page);
	}
}

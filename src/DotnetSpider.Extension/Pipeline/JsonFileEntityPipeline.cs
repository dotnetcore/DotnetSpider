using System.Collections.Generic;
using System.IO;
using DotnetSpider.Core;
using Newtonsoft.Json.Linq;
using System;

namespace DotnetSpider.Extension.Pipeline
{
	public class JsonFileEntityPipeline : BaseEntityPipeline
	{
		protected string DataFolder;

		public override void InitPipeline(ISpider spider)
		{
			base.InitPipeline(spider);

#if !NET_CORE
			DataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, spider.Identity, "entityJson");
#else
			DataFolder = Path.Combine(AppContext.BaseDirectory, spider.Identity, "entityJson");
#endif
		}

		public override void Process(string entityName, List<JObject> datas)
		{
			lock (this)
			{
				var fileInfo = PrepareFile(Path.Combine(DataFolder, $"{entityName}.data"));

				foreach (var entry in datas)
				{
					entry.Add("cdate", DateTime.Now);
					File.AppendAllText(fileInfo.Name, entry.ToString());
				}
			}
		}
	}
}

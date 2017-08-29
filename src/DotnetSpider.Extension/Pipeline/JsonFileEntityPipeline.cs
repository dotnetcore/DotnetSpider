using System.Collections.Generic;
using System.IO;
using DotnetSpider.Core;
using System;
using DotnetSpider.Extension.Model;

namespace DotnetSpider.Extension.Pipeline
{
	public class JsonFileEntityPipeline : BaseEntityPipeline
	{
		protected string DataFolder;

		public override void InitPipeline(ISpider spider)
		{
			base.InitPipeline(spider);

			DataFolder = Path.Combine(Core.Environment.BaseDirectory, spider.Identity, "entityJson");
		}

		public override void Process(string entityName, List<DataObject> datas)
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

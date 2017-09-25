using System.Collections.Generic;
using System.IO;
using DotnetSpider.Core;
using System;
using DotnetSpider.Extension.Model;

namespace DotnetSpider.Extension.Pipeline
{
	public class JsonFileEntityPipeline : BaseEntityPipeline
	{
		private readonly object _locker = new object();
		protected string DataFolder;

		public override void InitPipeline(ISpider spider)
		{
			base.InitPipeline(spider);

			lock (_locker)
			{
				DataFolder = Path.Combine(Env.BaseDirectory, spider.Identity, "entityJson");
			}
		}

		public override int Process(string entityName, List<DataObject> datas)
		{
			lock (_locker)
			{
				var fileInfo = PrepareFile(Path.Combine(DataFolder, $"{entityName}.data"));

				foreach (var entry in datas)
				{
					entry.Add("cdate", DateTime.Now);
					File.AppendAllText(fileInfo.Name, entry.ToString());
				}
				return datas.Count;
			}
		}
	}
}

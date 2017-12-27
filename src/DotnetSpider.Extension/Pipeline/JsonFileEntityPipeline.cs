using System.Collections.Generic;
using System.IO;
using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Extension.Model;

namespace DotnetSpider.Extension.Pipeline
{
	public class JsonFileEntityPipeline : BaseEntityPipeline
	{
		private readonly object _locker = new object();
		private string _dataFolder;

		public override void Init(ISpider spider)
		{
			base.Init(spider);

			lock (_locker)
			{
				_dataFolder = Path.Combine(Env.BaseDirectory, spider.Identity, "entityJson");
			}
		}

		public override int Process(string entityName, List<dynamic> datas)
		{
			lock (_locker)
			{
				var fileInfo = FileUtil.PrepareFile(Path.Combine(_dataFolder, $"{entityName}.json"));

				foreach (var entry in datas)
				{
					File.AppendAllText(fileInfo.Name, entry.ToString());
				}
				return datas.Count;
			}
		}

		public override void AddEntity(IEntityDefine type)
		{
		}
	}
}

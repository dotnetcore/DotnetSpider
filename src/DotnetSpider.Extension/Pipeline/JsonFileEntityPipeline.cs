using System.Collections.Generic;
using System.IO;
using DotnetSpider.Core;
using Newtonsoft.Json.Linq;
using System;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Extension.Model;

namespace DotnetSpider.Extension.Pipeline
{
	public class JsonFileEntityPipeline : BaseEntityPipeline
	{
		protected string DataFolder;
		protected StreamWriter Writer;

		private string _entityName;

		public override void InitiEntity(EntityMetadata metadata)
		{
			if (metadata.Schema == null)
			{
				IsEnabled = false;
				return;
			}
			_entityName = metadata.Entity.Name;
		}

		public override void InitPipeline(ISpider spider)
		{
			base.InitPipeline(spider);

#if !NET_CORE
			DataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, spider.Identity, "entityJson");
#else
			DataFolder = Path.Combine(AppContext.BaseDirectory, spider.Identity, "entityJson");
#endif
			Writer = BasePipeline.PrepareFile(Path.Combine(DataFolder, $"{_entityName}.data")).AppendText();
			Writer.AutoFlush = true;
		}

		public override void Process(List<JObject> datas)
		{
			lock (this)
			{
				foreach (var entry in datas)
				{
					Writer.WriteLine(entry);
				}
			}
		}

		public override BaseEntityPipeline Clone()
		{
			return  new JsonFileEntityPipeline();
		}

		public override void Dispose()
		{
			base.Dispose();
			Writer.Dispose();
		}
	}
}

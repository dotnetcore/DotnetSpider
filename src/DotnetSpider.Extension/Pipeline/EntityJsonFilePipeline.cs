using System.Collections.Generic;
using System.IO;
using DotnetSpider.Core;
using Newtonsoft.Json.Linq;
using System;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.ORM;

namespace DotnetSpider.Extension.Pipeline
{
	public class EntityJsonFilePipeline : EntityBasePipeline
	{
		protected string DataFolder;
		protected StreamWriter Writer;

		private string _entityName;

		public override void InitiEntity(Schema schema, EntityMetadata metadata)
		{
			_entityName = metadata.Name;
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

		public override void Dispose()
		{
			base.Dispose();
			Writer.Dispose();
		}
	}
}

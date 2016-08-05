using System.Collections.Generic;
using System.IO;
using System.Text;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Core.Utils;
using Newtonsoft.Json.Linq;
using System;
using Java2Dotnet.Spider.Core.Pipeline;

namespace Java2Dotnet.Spider.Extension.Pipeline
{
	public class EntityJsonFilePipeline : EntityBasePipeline
	{
		protected string DataFolder;
		protected StreamWriter Writer;

		private readonly string _entityName;

		public EntityJsonFilePipeline(JObject entityDefine)
		{
			_entityName = entityDefine.SelectToken("$.Identity").ToString();
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

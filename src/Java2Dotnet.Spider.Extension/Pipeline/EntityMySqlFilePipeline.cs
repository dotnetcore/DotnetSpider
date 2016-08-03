using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Core.Utils;
using Java2Dotnet.Spider.Extension.ORM;
using Newtonsoft.Json.Linq;
using Java2Dotnet.Spider.Extension.Configuration;
using System;
using Java2Dotnet.Spider.Core.Pipeline;

namespace Java2Dotnet.Spider.Extension.Pipeline
{
	/// <summary>
	/// LOAD DATA LOCAL INFILE '{filePath}' INTO TABLE `{schema}`.`{dababase}` FIELDS TERMINATED BY '$'  ENCLOSED BY '#' LINES TERMINATED BY '@END@' IGNORE 1 LINES;
	/// </summary>
	public class EntityMySqlFilePipeline : EntityBasePipeline
	{
		protected readonly Schema Schema;
		protected readonly List<DataToken> Columns;
		protected string DataFolder;
		protected StreamWriter Writer;

		public EntityMySqlFilePipeline(Schema schema, EntityMetadata entityDefine)
		{
			Schema = schema;
			Columns = entityDefine.Entity.Fields;
		}

		public override void InitPipeline(ISpider spider)
		{
			base.InitPipeline(spider);
#if !NET_CORE
			DataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, spider.Identity, "mysql");
#else
			DataFolder = Path.Combine(AppContext.BaseDirectory, spider.Identity, "mysql");
#endif
			Writer = BasePipeline.PrepareFile(Path.Combine(DataFolder, $"{Schema.Database}.{Schema.TableName}.data")).AppendText();
			Writer.AutoFlush = true;
		}

		public override void Process(List<JObject> datas)
		{
			lock (this)
			{

				StringBuilder builder = new StringBuilder();
				foreach (var entry in datas)
				{
					builder.Append("@END@");
					foreach (var column in Columns)
					{
						var value = entry.SelectToken($"$.{column.Name}")?.ToString();
						if (!string.IsNullOrEmpty(value))
						{
							builder.Append("#").Append(value).Append("#").Append("$");
						}
						else
						{
							builder.Append("##$");
						}
					}
				}
				Writer.Write( builder.ToString());
			}
		}

		public override void Dispose()
		{
			Writer.Dispose();
		}
	}
}

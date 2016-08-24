using System.Collections.Generic;
using System.IO;
using System.Text;
using DotnetSpider.Core;
using DotnetSpider.Extension.ORM;
using Newtonsoft.Json.Linq;
using System;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Extension.Model;
using NLog;

namespace DotnetSpider.Extension.Pipeline
{
	/// <summary>
	/// LOAD DATA LOCAL INFILE '{filePath}' INTO TABLE `{schema}`.`{dababase}` FIELDS TERMINATED BY '$'  ENCLOSED BY '#' LINES TERMINATED BY '@END@' IGNORE 1 LINES;
	/// </summary>
	public class MySqlFileEntityPipeline : BaseEntityPipeline
	{
		protected Schema Schema;
		protected List<DataToken> Columns;
		protected string DataFolder;
		protected StreamWriter Writer;

		public override void InitiEntity(EntityMetadata metadata)
		{
			if (metadata.Schema == null)
			{
				Logger.SaveLog(LogInfo.Create("Miss pipeline because: Schema is necessary", Logger.Name, Spider, LogLevel.Warn));
				IsEnabled = false;
				return;
			}
			Schema = metadata.Schema;
			Columns = metadata.Entity.Fields;
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
				Writer.Write(builder.ToString());
			}
		}

		public override void Dispose()
		{
			Writer.Dispose();
		}

		public override BaseEntityPipeline Clone()
		{
			return new MySqlFileEntityPipeline();
		}
	}
}

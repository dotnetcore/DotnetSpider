﻿using System.Collections.Generic;
using System.IO;
using System.Text;
using DotnetSpider.Core;
using DotnetSpider.Extension.ORM;
using Newtonsoft.Json.Linq;
using System;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Extension.Model;
using DotnetSpider.Core.Infrastructure;

namespace DotnetSpider.Extension.Pipeline
{
	/// <summary>
	/// LOAD DATA LOCAL INFILE '{filePath}' INTO TABLE `{schema}`.`{dababase}` FIELDS TERMINATED BY '$'  ENCLOSED BY '#' LINES TERMINATED BY '@END@' IGNORE 1 LINES;
	/// </summary>
	public class MySqlFileEntityPipeline : BaseEntityPipeline
	{
		protected string DataFolder;

		public override void InitPipeline(ISpider spider)
		{
			base.InitPipeline(spider);
#if !NET_CORE
			DataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, spider.Identity, "mysql");
#else
			DataFolder = Path.Combine(AppContext.BaseDirectory, spider.Identity, "mysql");
#endif
		}

		public override void Process(string entityName, List<JObject> datas)
		{
			lock (this)
			{
				EntityMetadata metadata;
				if (EntityMetadatas.TryGetValue(entityName, out metadata))
				{
					var fileInfo = PrepareFile(Path.Combine(DataFolder, $"{metadata.Table.Database}.{metadata.Table.Name}.data"));
					StringBuilder builder = new StringBuilder();
					foreach (var entry in datas)
					{
						builder.Append("@END@");
						foreach (var column in metadata.Entity.Fields)
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
					File.AppendAllText(fileInfo.Name, builder.ToString());
				}
			}
		}
	}
}

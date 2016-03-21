using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Core.Utils;
using Java2Dotnet.Spider.Extension.ORM;
using Newtonsoft.Json.Linq;

namespace Java2Dotnet.Spider.Extension.Pipeline
{
	/// <summary>
	/// LOAD DATA LOCAL INFILE '{filePath}' INTO TABLE `{schema}`.`{dababase}` FIELDS TERMINATED BY '$'  ENCLOSED BY '#' LINES TERMINATED BY '@END@' IGNORE 1 LINES;
	/// </summary>
	public class EntityMySqlFilePipeline : FilePersistentBase, IEntityPipeline
	{
		protected readonly Schema Schema;
		protected readonly List<EntityGeneralPipeline.Column> Columns;

		public EntityMySqlFilePipeline(Schema schema, JObject entityDefine)
		{
			Schema = schema;

			Columns = entityDefine.SelectTokens("$.Fields[*]").Select(j => j.ToObject<EntityGeneralPipeline.Column>()).ToList();
			SetPath("DataFiles");
		}

		public void Initialize()
		{
		}

		public void Process(List<JObject> datas, ISpider spider)
		{
			lock (this)
			{
				FileInfo file = PrepareFile(BasePath + $"{Schema.Database}.{Schema.TableName}.df");

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
				File.AppendAllText(file.FullName, builder.ToString());
			}
		}

		public void Dispose()
		{
		}
	}
}

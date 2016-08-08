using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using DotnetSpider.Core;
using Newtonsoft.Json.Linq;
using DotnetSpider.Extension.ORM;
using DotnetSpider.Core.Common;
using DotnetSpider.Extension.Configuration;

namespace DotnetSpider.Extension.Pipeline
{
	public abstract class EntityGeneralPipeline : EntityBasePipeline
	{
		//public class Column
		//{
		//	public string Name { get; set; }
		//	public string DataType { get; set; }

		//	public override string ToString()
		//	{
		//		return $"{Name},{DataType}";
		//	}
		//}

		protected string ConnectString { get; set; }
		protected readonly List<Field> Columns = new List<Field>();
		protected readonly List<Field> UpdateColumns = new List<Field>();

		protected abstract DbConnection CreateConnection();

		protected abstract string GetInsertSql();
		protected abstract string GetUpdateSql();
		protected abstract string GetCreateTableSql();
		protected abstract string GetCreateSchemaSql();
		protected abstract DbParameter CreateDbParameter();
		protected readonly Schema Schema;
		protected PipelineMode Mode { get; set; }

		//protected readonly Type Type;

		protected List<List<string>> Indexs { get; set; } = new List<List<string>>();
		protected List<List<string>> Uniques { get; set; } = new List<List<string>>();
		protected List<Field> Primary { get; set; } = new List<Field>();
		protected string AutoIncrement { get; set; }

		protected abstract string ConvertToDbType(string datatype);

		protected EntityGeneralPipeline(Schema schema, EntityMetadata entityDefine, string connectString, PipelineMode mode = PipelineMode.Insert)
		{
			Mode = mode;
			ConnectString = connectString;

			Schema = GenerateSchema(schema);
			foreach (var f in entityDefine.Entity.Fields)
			{
				if (!string.IsNullOrEmpty(((Field)f).DataType))
				{
					Columns.Add((Field)f);
				}
			}
			var primary = entityDefine.Primary;
			if (primary != null)
			{
				foreach (var p in primary)
				{
					var col = Columns.FirstOrDefault(c => c.Name == p);
					if (col == null)
					{
						throw new SpiderException("Columns set as primary is not a property of your entity.");
					}
					else
					{
						Primary.Add(col);
					}
				}
			}

			if (mode == PipelineMode.Update && entityDefine.Updates != null)
			{
				foreach (var column in entityDefine.Updates)
				{
					var col = Columns.FirstOrDefault(c => c.Name == column);
					if (col == null)
					{
						throw new SpiderException("Columns set as update is not a property of your entity.");
					}
					else
					{
						UpdateColumns.Add(col);
					}
				}
				if (UpdateColumns == null || UpdateColumns.Count == 0)
				{
					UpdateColumns = Columns;
					UpdateColumns.RemoveAll(c => Primary.Contains(c));
				}
				if (Primary == null || Primary.Count == 0)
				{
					throw new SpiderException("Do you forget set the Primary in IndexesAttribute for your entity class.");
				}
			}

			AutoIncrement = entityDefine.AutoIncrement;

			if (entityDefine.Indexes != null)
			{
				foreach (var index in entityDefine.Indexes)
				{
					List<string> tmpIndex = new List<string>();
					foreach (var i in index)
					{
						var col = Columns.FirstOrDefault(c => c.Name == i);
						if (col == null)
						{
							throw new SpiderException("Columns set as index is not a property of your entity.");
						}
						else
						{
							tmpIndex.Add(col.Name);
						}
					}
					if (tmpIndex.Count != 0)
					{
						Indexs.Add(tmpIndex);
					}
				}
			}
			if (entityDefine.Uniques != null)
			{
				foreach (var unique in entityDefine.Uniques)
				{
					List<string> tmpUnique = new List<string>();
					foreach (var i in unique)
					{
						var col = Columns.FirstOrDefault(c => c.Name == i);
						if (col == null)
						{
							throw new SpiderException("Columns set as unique is not a property of your entity.");
						}
						else
						{
							tmpUnique.Add(col.Name);
						}
					}
					if (tmpUnique.Count != 0)
					{
						Uniques.Add(tmpUnique);
					}
				}
			}
		}

		private Schema GenerateSchema(Schema schema)
		{
			switch (schema.Suffix)
			{
				case TableSuffix.FirstDayOfThisMonth:
					{
						schema.TableName += "_" + DateTimeUtils.FirstDayofThisMonth.ToString("yyyy_MM_dd");
						break;
					}
				case TableSuffix.Monday:
					{
						schema.TableName += "_" + DateTimeUtils.FirstDayofThisWeek.ToString("yyyy_MM_dd");
						break;
					}
				case TableSuffix.Today:
					{
						schema.TableName += "_" + DateTime.Now.ToString("yyyy_MM_dd");
						break;
					}
			}
			return schema;
		}

		public override void InitPipeline(ISpider spider)
		{
			base.InitPipeline(spider);

			if (Mode == PipelineMode.Update)
			{
				return;
			}
			NetworkProxyManager.Current.Execute("db-init", () =>
			{
				using (DbConnection conn = CreateConnection())
				{
					conn.Open();
					var command = conn.CreateCommand();
					command.CommandText = GetCreateSchemaSql();
					command.CommandType = CommandType.Text;
					command.ExecuteNonQuery();

					command.CommandText = GetCreateTableSql();
					command.CommandType = CommandType.Text;
					command.ExecuteNonQuery();
					conn.Close();
				}
			});
		}

		public override void Process(List<JObject> datas)
		{
			NetworkProxyManager.Current.Execute("pipeline-", () =>
			{
				switch (Mode)
				{
					case PipelineMode.Insert:
						{
							using (var conn = CreateConnection())
							{
								var cmd = conn.CreateCommand();
								cmd.CommandText = GetInsertSql();
								cmd.CommandType = CommandType.Text;
								conn.Open();

								foreach (var data in datas)
								{
									cmd.Parameters.Clear();

									List<DbParameter> parameters = new List<DbParameter>();
									foreach (var column in Columns)
									{
										var parameter = CreateDbParameter();
										parameter.ParameterName = $"@{column.Name}";
										parameter.Value = data.SelectToken($"{column.Name}")?.Value<string>();
										parameter.DbType = Convert(column.DataType);
										parameters.Add(parameter);
									}

									cmd.Parameters.AddRange(parameters.ToArray());
									cmd.ExecuteNonQuery();
								}

								conn.Close();
							}
							break;
						}
					case PipelineMode.Update:
						{
							using (var conn = CreateConnection())
							{
								var cmd = conn.CreateCommand();
								cmd.CommandText = GetUpdateSql();
								cmd.CommandType = CommandType.Text;
								conn.Open();

								foreach (var data in datas)
								{
									cmd.Parameters.Clear();

									List<DbParameter> parameters = new List<DbParameter>();
									foreach (var column in UpdateColumns)
									{
										var parameter = CreateDbParameter();
										parameter.ParameterName = $"@{column.Name}";
										parameter.Value = data.SelectToken($"{column.Name}")?.Value<string>();
										parameter.DbType = Convert(column.DataType);
										parameters.Add(parameter);
									}

									foreach (var column in Primary)
									{
										var parameter = CreateDbParameter();
										parameter.ParameterName = $"@{column.Name}";
										parameter.Value = data.SelectToken($"{column.Name}")?.Value<string>();
										parameter.DbType = Convert(column.DataType);
										parameters.Add(parameter);
									}

									cmd.Parameters.AddRange(parameters.ToArray());
									cmd.ExecuteNonQuery();
								}

								conn.Close();
							}
							break;
						}
				}

			});
		}

		private DbType Convert(string type)
		{
			if (string.IsNullOrEmpty(type))
			{
				throw new SpiderException("TYPE can not be null");
			}

			string datatype = type.ToLower();
			if (RegexUtil.StringTypeRegex.IsMatch(datatype) || "text" == datatype)
			{
				return DbType.String;
			}
			if ("bool" == datatype)
			{
				return DbType.Boolean;
			}

			if ("date" == datatype || "time" == datatype)
			{
				return DbType.DateTime;
			}

			throw new SpiderException("Unsport datatype: " + datatype);
		}
	}
}


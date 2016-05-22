using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Java2Dotnet.Spider.Core;
using Newtonsoft.Json.Linq;
using Java2Dotnet.Spider.JLog;
using Java2Dotnet.Spider.Extension.ORM;
using Java2Dotnet.Spider.Extension.Utils;
using Java2Dotnet.Spider.Common;
using Java2Dotnet.Spider.Redial;
using Java2Dotnet.Spider.Extension.Configuration;

namespace Java2Dotnet.Spider.Extension.Pipeline
{
	public abstract class EntityGeneralPipeline : IEntityPipeline
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
		protected readonly List<Field> Columns;
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

		protected EntityGeneralPipeline(Schema schema, Entity entityDefine, string connectString, PipelineMode mode = PipelineMode.Insert)
		{
			Mode = mode;
			ConnectString = connectString;

			Schema = GenerateSchema(schema);
			Columns = entityDefine.Fields.Where(f => f.DataType != null).ToList();
			var primary = entityDefine.Primary;
			if (primary != null)
			{
				foreach (var p in primary)
				{
					var col = Columns.FirstOrDefault(c => c.Name == p);
					if (col == null)
					{
						throw new SpiderExceptoin("Columns set as primary is not a property of your entity.");
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
						throw new SpiderExceptoin("Columns set as update is not a property of your entity.");
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
					throw new SpiderExceptoin("Do you forget set the Primary in IndexesAttribute for your entity class.");
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
							throw new SpiderExceptoin("Columns set as index is not a property of your entity.");
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
							throw new SpiderExceptoin("Columns set as unique is not a property of your entity.");
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

		public virtual void Initialize()
		{
			if (Mode == PipelineMode.Update)
			{
				return;
			}
			RedialManagerUtils.Execute("db-init", () =>
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

		public void Process(List<JObject> datas, ISpider spider)
		{
			RedialManagerUtils.Execute("pipeline-", () =>
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

		public void Dispose()
		{
		}


		//private void GenerateType(Schema schema, List<Column> columns)
		//{
		//	AppDomain currentAppDomain = AppDomain.CurrentDomain;
		//	AssemblyName assyName = new AssemblyName("DotnetSpiderAss_" + schema.TableName);

		//	AssemblyBuilder assyBuilder = currentAppDomain.DefineDynamicAssembly(assyName, AssemblyBuilderAccess.Run);

		//	ModuleBuilder modBuilder = assyBuilder.DefineDynamicModule("DotnetSpiderMod_" + schema.TableName);

		//	TypeBuilder typeBuilder = modBuilder.DefineType("type_" + schema.TableName, TypeAttributes.Class | TypeAttributes.Public);

		//	foreach (var column in columns)
		//	{
		//		AddProperty(typeBuilder, column.Name, Convert(column.DataType.ToLower()));
		//	}

		//	return (typeBuilder.CreateType());
		//}

		//private Type GenerateType(Schema schema, List<Column> columns)
		//{
		//	AssemblyName assyName = new AssemblyName("DotnetSpiderAss_" + schema.TableName);

		//	AssemblyBuilder assyBuilder = AssemblyBuilder.DefineDynamicAssembly(assyName, AssemblyBuilderAccess.Run);

		//	ModuleBuilder modBuilder = assyBuilder.DefineDynamicModule("DotnetSpiderMod_" + schema.TableName);

		//	TypeBuilder typeBuilder = modBuilder.DefineType("type_" + schema.TableName, TypeAttributes.Class | TypeAttributes.Public);

		//	foreach (var column in columns)
		//	{
		//		AddProperty(typeBuilder, column.Name, Convert(column.DataType.ToLower()));
		//	}

		//	return (typeBuilder.CreateTypeInfo().AsType());
		//}


		//private void AddProperty(TypeBuilder tb, string name, Type type)
		//{
		//	var property = tb.DefineProperty(name, PropertyAttributes.HasDefault, type, null);

		//	FieldBuilder field = tb.DefineField($"_{name}", type, FieldAttributes.Private);

		//	MethodAttributes getOrSetAttribute = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

		//	MethodBuilder getAccessor = tb.DefineMethod($"get_{name}", getOrSetAttribute, type, Type.EmptyTypes);

		//	ILGenerator getIl = getAccessor.GetILGenerator();
		//	getIl.Emit(OpCodes.Ldarg_0);
		//	getIl.Emit(OpCodes.Ldfld, field);
		//	getIl.Emit(OpCodes.Ret);

		//	MethodBuilder setAccessor = tb.DefineMethod($"set_{name}", getOrSetAttribute, null, new[] { type });

		//	ILGenerator setIl = setAccessor.GetILGenerator();
		//	setIl.Emit(OpCodes.Ldarg_0);
		//	setIl.Emit(OpCodes.Ldarg_1);
		//	setIl.Emit(OpCodes.Stfld, field);
		//	setIl.Emit(OpCodes.Ret);

		//	property.SetGetMethod(getAccessor);
		//	property.SetSetMethod(setAccessor);
		//}

		private DbType Convert(string type)
		{
			if (string.IsNullOrEmpty(type))
			{
				throw new SpiderExceptoin("TYPE can not be null");
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


			throw new SpiderExceptoin("Unsport datatype: " + datatype);
		}
	}
}


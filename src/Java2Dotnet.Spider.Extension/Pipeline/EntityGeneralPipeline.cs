using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Java2Dotnet.Spider.Core;
using Newtonsoft.Json.Linq;

using Java2Dotnet.Spider.Extension.ORM;
using Java2Dotnet.Spider.Extension.Utils;
using Java2Dotnet.Spider.Common;
using Java2Dotnet.Spider.Redial;
#if !NET_CORE
using log4net;
//using Dapper;
#else
using Java2Dotnet.Spider.JLog; 
#endif
using PropertyAttributes = System.Reflection.PropertyAttributes;

namespace Java2Dotnet.Spider.Extension.Pipeline
{
	public abstract class EntityGeneralPipeline : IEntityPipeline
	{
#if !NET_CORE
		protected static ILog Logger = LogManager.GetLogger(typeof(EntityGeneralPipeline));
#else
		protected static ILog Logger = LogManager.GetLogger();
#endif
		public class Column
		{
			public string Name { get; set; }
			public string DataType { get; set; }

			public override string ToString()
			{
				return $"{Name},{DataType}";
			}
		}

		protected string ConnectString { get; set; }
		protected readonly List<Column> Columns;

		protected abstract DbConnection CreateConnection();

		protected abstract string GetInsertSql();
		protected abstract string GetCreateTableSql();
		protected abstract string GetCreateSchemaSql();
		protected abstract DbParameter CreateDbParameter();
		protected readonly Schema Schema;

		//protected readonly Type Type;

		protected List<List<string>> Indexs { get; set; } = new List<List<string>>();
		protected List<List<string>> Uniques { get; set; } = new List<List<string>>();
		protected List<string> Primary { get; set; }
		protected string AutoIncrement { get; set; }

		protected abstract string ConvertToDbType(string datatype);

		protected EntityGeneralPipeline(Schema schema, JObject entityDefine, string connectString)
		{
#if NET_CORE
			Logger.Info($"Db ConnectString: {connectString}", true);
#endif

			ConnectString = connectString;

			Schema = GenerateSchema(schema);
			Columns = entityDefine.SelectTokens("$.Fields[*]").Select(j => j.ToObject<Column>()).Where(c => !string.IsNullOrEmpty(c.DataType)).ToList();

			Primary = entityDefine.SelectToken("$.Primary").ToObject<List<string>>();
			AutoIncrement = entityDefine.SelectToken("$.AutoIncrement")?.ToString();
			foreach (var index in entityDefine.SelectTokens("$.Indexs[*]"))
			{
				Indexs.Add(index.ToObject<List<string>>());
			}

			foreach (var index in entityDefine.SelectTokens("$.Uniques[*]"))
			{
				Uniques.Add(index.ToObject<List<string>>());
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
			RedialManagerUtils.Execute("db-insert", () =>
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


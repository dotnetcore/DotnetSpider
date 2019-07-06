using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DotnetSpider.DataFlow;
using DotnetSpider.DataFlow.Parser;
using DotnetSpider.DataFlow.Storage;
using DotnetSpider.DataFlow.Storage.Model;
using MySql.Data.MySqlClient;
using Xunit;

namespace DotnetSpider.Tests.Data.Storage
{
	public class MySqlEntityStorageTests : TestBase
	{
		protected virtual string Escape => "`";

		class IndexInfo
		{
			// ReSharper disable once InconsistentNaming
			// ReSharper disable once UnusedAutoPropertyAccessor.Local
			public int Non_unique { get; set; }
			
			// ReSharper disable once InconsistentNaming			
			// ReSharper disable once UnusedAutoPropertyAccessor.Local
			public string Key_name { get; set; }
			
			// ReSharper disable once InconsistentNaming
			// ReSharper disable once UnusedAutoPropertyAccessor.Local
			public string Column_name { get; set; }
		}

		class PrimaryInfo
		{
			// ReSharper disable once InconsistentNaming
			// ReSharper disable once UnusedAutoPropertyAccessor.Local
			public string COLUMN_NAME { get; set; }
		}


		protected virtual string GetConnectionString()
		{
			return
				"Database='mysql';Data Source=localhost;password=1qazZAQ!;User ID=root;Port=3306;SslMode=None;Allow User Variables=True;AllowPublicKeyRetrieval=True";
		}

		protected virtual IDbConnection CreateConnection()
		{
			return new MySqlConnection(GetConnectionString());
		}

		protected virtual StorageBase CreateStorage(StorageType type)
		{
			return new MySqlEntityStorage(type, GetConnectionString());
		}

		/// <summary>
		/// 测试能正确创建 MySql 表
		/// 1. 如果实体的 Schema 没有配置表名，则使用类名
		/// 2. 如果实体的 Schema 配置了表名，则使用配置的表名
		/// 3. 是否有正确添加表的后缀
		/// </summary>
		[Fact(DisplayName = "CreateTableWhenNoSchema")]
		public async Task CreateTableWhenNoSchema()
		{
			using (var conn = CreateConnection())
			{
				// 如果实体的 Schema 没有配置表名，则使用类名
				await conn.ExecuteAsync($"drop table if exists createtableentity1;");
				var services = SpiderProvider.Value.CreateScopeServiceProvider();
				var storage = CreateStorage(StorageType.Insert);
				var dfc = new DataFlowContext(null, services);
				var typeName = typeof(CreateTableEntity1).FullName;
				var entity = new CreateTableEntity1();
				dfc.Add(typeName, entity.GetTableMetadata());
				var items = new ParseResult<CreateTableEntity1>
				{
					entity
				};
				dfc.AddParseItem(typeName, items);
				await storage.HandleAsync(dfc);
				var list = (await conn.QueryAsync<CreateTableEntity1>($"SELECT * FROM createtableentity1")).ToList();
				Assert.Single(list);
				entity = list.First();
				Assert.Equal("xxx", entity.Str1);
				Assert.Equal("yyy", entity.Str2);
				Assert.Equal(655, entity.Required);
				Assert.Equal(0, entity.Decimal);
				Assert.Equal(600, entity.Long);
				Assert.Equal(400, entity.Double);
				Assert.Equal(200.0F, entity.Float);
				await conn.ExecuteAsync($"drop table if exists createtableentity1;");
			}
		}

		[Fact(DisplayName = "CreateTableWhenNoTableNameInSchema")]
		public async Task CreateTableWhenNoTableNameInSchema()
		{
			using (var conn = CreateConnection())
			{
				// 如果实体的 Schema 没有配置表名，则使用类名
				await conn.ExecuteAsync($"drop table if exists createtablenotablename;");
				var services = SpiderProvider.Value.CreateScopeServiceProvider();
				var storage = CreateStorage(StorageType.Insert);
				var dfc = new DataFlowContext(null, services);
				var typeName = typeof(CreateTableEntity2).FullName;
				var entity = new CreateTableEntity2();
				dfc.Add(typeName, entity.GetTableMetadata());
				var items = new ParseResult<CreateTableEntity2>
				{
					entity
				};
				dfc.AddParseItem(typeName, items);
				await storage.HandleAsync(dfc);
				var list = (await conn.QueryAsync<CreateTableEntity2>($"SELECT * FROM createtablenotablename")).ToList();
				Assert.Single(list);
				entity = list.First();
				Assert.Equal("xxx", entity.Str1);
				Assert.Equal("yyy", entity.Str2);
				Assert.Equal(655, entity.Required);
				Assert.Equal(0, entity.Decimal);
				Assert.Equal(600, entity.Long);
				Assert.Equal(400, entity.Double);
				Assert.Equal(200.0F, entity.Float);
				await conn.ExecuteAsync($"drop table if exists createtablenotablename;");
			}
		}

		[Fact(DisplayName = "CreateTable")]
		public async Task CreateTable()
		{
			using (var conn = CreateConnection())
			{
				// 如果实体的 Schema 没有配置表名，则使用类名
				await conn.ExecuteAsync($"drop table if exists {Escape}test{Escape}.{Escape}createtable{Escape};");
				var services = SpiderProvider.Value.CreateScopeServiceProvider();
				var storage = CreateStorage(StorageType.Insert);
				var dfc = new DataFlowContext(null, services);
				var typeName = typeof(CreateTableEntity3).FullName;
				var entity = new CreateTableEntity3();
				dfc.Add(typeName, entity.GetTableMetadata());
				var items = new ParseResult<CreateTableEntity3>
				{
					entity
				};
				dfc.AddParseItem(typeName, items);
				await storage.HandleAsync(dfc);
				var list = (await conn.QueryAsync<CreateTableEntity3>(
					$"SELECT * FROM {Escape}test{Escape}.{Escape}createtable{Escape}")).ToList();
				Assert.Single(list);
				entity = list.First();
				Assert.Equal("xxx", entity.Str1);
				Assert.Equal("yyy", entity.Str2);
				Assert.Equal(655, entity.Required);
				Assert.Equal(0, entity.Decimal);
				Assert.Equal(600, entity.Long);
				Assert.Equal(400, entity.Double);
				Assert.Equal(200.0F, entity.Float);
				await conn.ExecuteAsync($"drop table if exists {Escape}test{Escape}.{Escape}createtable{Escape};");
			}
		}

		[Fact(DisplayName = "MultiPrimary")]
		public async Task MultiPrimary()
		{
			using (var conn = CreateConnection())
			{
				// 如果实体的 Schema 没有配置表名，则使用类名
				await conn.ExecuteAsync(
					$"drop table if exists {Escape}test{Escape}.{Escape}createtablemultiprimay{Escape};");
				var services = SpiderProvider.Value.CreateScopeServiceProvider();
				var storage = CreateStorage(StorageType.Insert);
				var dfc = new DataFlowContext(null, services);
				var typeName = typeof(CreateTableEntity8).FullName;
				var entity = new CreateTableEntity8();
				dfc.Add(typeName, entity.GetTableMetadata());
				var items = new ParseResult<CreateTableEntity8>
				{
					entity
				};
				dfc.AddParseItem(typeName, items);
				await storage.HandleAsync(dfc);
				var list = (await conn.QueryAsync<CreateTableEntity8>(
						$"SELECT * FROM {Escape}test{Escape}.{Escape}createtablemultiprimay{Escape}"))
					.ToList();
				Assert.Single(list);
				entity = list.First();
				Assert.Equal("xxx", entity.Str1);
				Assert.Equal("yyy", entity.Str2);
				Assert.Equal(655, entity.Required);
				Assert.Equal(0, entity.Decimal);
				Assert.Equal(600, entity.Long);
				Assert.Equal(400, entity.Double);
				Assert.Equal(200.0F, entity.Float);

				var primaries = (await conn.QueryAsync<PrimaryInfo>(
						$"SELECT t.CONSTRAINT_TYPE, c.COLUMN_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS t, INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS c WHERE t.TABLE_NAME = c.TABLE_NAME AND t.TABLE_SCHEMA = 'test' AND t.CONSTRAINT_TYPE = 'PRIMARY KEY' AND t.TABLE_NAME='createtablemultiprimay';")
					).ToList();
				Assert.Equal(2, primaries.Count);
				Assert.Equal("str2", primaries[0].COLUMN_NAME);
				Assert.Equal("decimal", primaries[1].COLUMN_NAME);
				await conn.ExecuteAsync(
					$"drop table if exists {Escape}test{Escape}.{Escape}createtablemultiprimay{Escape};");
			}
		}

		[Fact(DisplayName = "Primary")]
		public async Task Primary()
		{
			using (var conn = CreateConnection())
			{
				// 如果实体的 Schema 没有配置表名，则使用类名
				await conn.ExecuteAsync(
					$"drop table if exists {Escape}test{Escape}.{Escape}createtableprimay{Escape};");
				var services = SpiderProvider.Value.CreateScopeServiceProvider();
				var storage = CreateStorage(StorageType.Insert);
				var dfc = new DataFlowContext(null, services);
				var typeName = typeof(CreateTableEntity4).FullName;
				var entity = new CreateTableEntity4();
				dfc.Add(typeName, entity.GetTableMetadata());
				var items = new ParseResult<CreateTableEntity4>
				{
					entity
				};
				dfc.AddParseItem(typeName, items);
				await storage.HandleAsync(dfc);
				var list = (await conn.QueryAsync<CreateTableEntity4>(
						$"SELECT * FROM {Escape}test{Escape}.{Escape}createtableprimay{Escape}"))
					.ToList();
				Assert.Single(list);
				entity = list.First();
				Assert.Equal("xxx", entity.Str1);
				Assert.Equal("yyy", entity.Str2);
				Assert.Equal(655, entity.Required);
				Assert.Equal(0, entity.Decimal);
				Assert.Equal(600, entity.Long);
				Assert.Equal(400, entity.Double);
				Assert.Equal(200.0F, entity.Float);

				var primaries = (await conn.QueryAsync<PrimaryInfo>(
						$"SELECT t.CONSTRAINT_TYPE, c.COLUMN_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS t, INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS c WHERE t.TABLE_NAME = c.TABLE_NAME AND t.TABLE_SCHEMA = 'test' AND t.CONSTRAINT_TYPE = 'PRIMARY KEY' AND t.TABLE_NAME='createtableprimay';")
					).ToList();
				Assert.Single(primaries);
				Assert.Equal("str2", primaries[0].COLUMN_NAME);
				await conn.ExecuteAsync(
					$"drop table if exists {Escape}test{Escape}.{Escape}createtableprimay{Escape};");
			}
		}

		[Fact(DisplayName = "AutoIncPrimary")]
		public async Task AutoIncPrimary()
		{
			using (var conn = CreateConnection())
			{
				// 如果实体的 Schema 没有配置表名，则使用类名
				await conn.ExecuteAsync(
					$"drop table if exists {Escape}test{Escape}.{Escape}createtableautoincprimay{Escape};");
				var services = SpiderProvider.Value.CreateScopeServiceProvider();
				var storage = CreateStorage(StorageType.Insert);
				var dfc = new DataFlowContext(null, services);
				var typeName = typeof(CreateTableEntity5).FullName;
				var entity = new CreateTableEntity5();
				dfc.Add(typeName, entity.GetTableMetadata());
				var items = new ParseResult<CreateTableEntity5>
				{
					entity,
					entity
				};
				dfc.AddParseItem(typeName, items);
				await storage.HandleAsync(dfc);
				var list =
					(await conn.QueryAsync<CreateTableEntity5>(
						$"SELECT * FROM {Escape}test{Escape}.{Escape}createtableautoincprimay{Escape}"))
					.ToList();
				Assert.Equal(2, list.Count);
				entity = list.First();
				Assert.Equal("xxx", entity.Str1);
				Assert.Equal("yyy", entity.Str2);
				Assert.Equal(655, entity.Required);
				Assert.Equal(0, entity.Decimal);
				Assert.Equal(600, entity.Long);
				Assert.Equal(400, entity.Double);
				Assert.Equal(200.0F, entity.Float);
				Assert.Equal(1, entity.Id);
				Assert.Equal(2, list[1].Id);

				var primaries = (await conn.QueryAsync<PrimaryInfo>(
						$"SELECT t.CONSTRAINT_TYPE, c.COLUMN_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS t, INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS c WHERE t.TABLE_NAME = c.TABLE_NAME AND t.TABLE_SCHEMA = 'test' AND t.CONSTRAINT_TYPE = 'PRIMARY KEY' AND t.TABLE_NAME='createtableautoincprimay';")
					).ToList();
				Assert.Single(primaries);
				Assert.Equal("id", primaries[0].COLUMN_NAME);
				await conn.ExecuteAsync(
					$"drop table if exists {Escape}test{Escape}.{Escape}createtableautoincprimay{Escape};");
			}
		}

		/// <summary>
		/// 测试能正确插入数据
		/// </summary>
		[Fact(DisplayName = "Insert")]
		public async Task Insert()
		{
			await CreateTable();
		}

		/// <summary>
		/// 测试能正确插入数据，如果遇到重复数据则忽略插入
		/// </summary>
		[Fact(DisplayName = "InsertIgnoreDuplicate")]
		public async Task InsertIgnoreDuplicate()
		{
			using (var conn = CreateConnection())
			{
				// 如果实体的 Schema 没有配置表名，则使用类名
				await conn.ExecuteAsync(
					$"drop table if exists {Escape}test{Escape}.{Escape}createtableprimay{Escape};");
				var services = SpiderProvider.Value.CreateScopeServiceProvider();
				var storage = CreateStorage(StorageType.InsertIgnoreDuplicate);
				var dfc = new DataFlowContext(null, services);
				var typeName = typeof(CreateTableEntity4).FullName;
				var entity = new CreateTableEntity4();
				dfc.Add(typeName, entity.GetTableMetadata());
				var items = new ParseResult<CreateTableEntity4>
				{
					entity,
					entity,
					entity
				};
				dfc.AddParseItem(typeName, items);
				await storage.HandleAsync(dfc);
				var list = (await conn.QueryAsync<CreateTableEntity4>(
						$"SELECT * FROM {Escape}test{Escape}.{Escape}createtableprimay{Escape}"))
					.ToList();
				Assert.Single(list);
				entity = list.First();
				Assert.Equal("xxx", entity.Str1);
				Assert.Equal("yyy", entity.Str2);
				Assert.Equal(655, entity.Required);
				Assert.Equal(0, entity.Decimal);
				Assert.Equal(600, entity.Long);
				Assert.Equal(400, entity.Double);
				Assert.Equal(200.0F, entity.Float);

				await conn.ExecuteAsync(
					$"drop table if exists {Escape}test{Escape}.{Escape}createtableprimay{Escape};");
			}
		}

		/// <summary>
		/// 测试 如果遇到重复数据则更新，主键不重复则插入
		/// 1. 此模式必须配置有主键，无主键效率太低
		/// 2. 更新则是全量更新
		/// </summary>
		[Fact(DisplayName = "InsertAndUpdate")]
		public async Task InsertAndUpdate()
		{
			using (var conn = CreateConnection())
			{
				// 如果实体的 Schema 没有配置表名，则使用类名
				await conn.ExecuteAsync(
					$"drop table if exists {Escape}test{Escape}.{Escape}createtableprimay{Escape};");
				var services = SpiderProvider.Value.CreateScopeServiceProvider();
				var storage = CreateStorage(StorageType.InsertAndUpdate);
				var dfc = new DataFlowContext(null, services);
				var typeName = typeof(CreateTableEntity4).FullName;
				var entity = new CreateTableEntity4();
				dfc.Add(typeName, entity.GetTableMetadata());
				var items = new ParseResult<CreateTableEntity4>
				{
					entity,
					new CreateTableEntity4
					{
						Str1 = "zzz"
					}
				};
				dfc.AddParseItem(typeName, items);

				await storage.HandleAsync(dfc);
				var list = (await conn.QueryAsync<CreateTableEntity4>(
						$"SELECT * FROM {Escape}test{Escape}.{Escape}createtableprimay{Escape}"))
					.ToList();
				Assert.Single(list);
				entity = list.First();
				Assert.Equal("zzz", entity.Str1);
				Assert.Equal("yyy", entity.Str2);
				Assert.Equal(655, entity.Required);
				Assert.Equal(0, entity.Decimal);
				Assert.Equal(600, entity.Long);
				Assert.Equal(400, entity.Double);
				Assert.Equal(200.0F, entity.Float);

				await conn.ExecuteAsync(
					$"drop table if exists {Escape}test{Escape}.{Escape}createtableprimay{Escape};");
			}
		}

		/// <summary>
		/// 测试能否正确更新数据
		/// </summary>
		[Fact(DisplayName = "UpdateAllColumns")]
		public async Task UpdateAllColumns()
		{
			using (var conn = CreateConnection())
			{
				// 如果实体的 Schema 没有配置表名，则使用类名
				await conn.ExecuteAsync(
					$"drop table if exists {Escape}test{Escape}.{Escape}createtableprimay{Escape};");
				var services = SpiderProvider.Value.CreateScopeServiceProvider();
				var storage = CreateStorage(StorageType.InsertIgnoreDuplicate);
				var dfc = new DataFlowContext(null, services);
				var typeName = typeof(CreateTableEntity4).FullName;
				var entity = new CreateTableEntity4();
				dfc.Add(typeName, entity.GetTableMetadata());
				var items = new ParseResult<CreateTableEntity4>
				{
					entity
				};
				dfc.AddParseItem(typeName, items);

				await storage.HandleAsync(dfc);

				var dfc2 = new DataFlowContext(null, services);
				dfc2.Add(typeName, entity.GetTableMetadata());
				var now = DateTime.Now;
				dfc2.AddParseItem(typeName, new ParseResult<CreateTableEntity4>
				{
					new CreateTableEntity4
					{
						Str1 = "TTT",
						DateTime = now,
						DateTimeOffset = now,
						Double = 888
					}
				});
				var storage2 = CreateStorage(StorageType.Update);
				await storage2.HandleAsync(dfc2);

				var list = (await conn.QueryAsync<CreateTableEntity4>(
						$"SELECT * FROM {Escape}test{Escape}.{Escape}createtableprimay{Escape}"))
					.ToList();
				Assert.Single(list);
				entity = list.First();
				Assert.Equal("TTT", entity.Str1);
				Assert.Equal("yyy", entity.Str2);
				Assert.Equal(655, entity.Required);
				Assert.Equal(0, entity.Decimal);
				Assert.Equal(600, entity.Long);
				Assert.Equal(888, entity.Double);
				Assert.Equal(200.0F, entity.Float);

				await conn.ExecuteAsync(
					$"drop table if exists {Escape}test{Escape}.{Escape}createtableprimay{Escape};");
			}
		}

		/// <summary>
		/// 测试能否正确更新数据
		/// </summary>
		[Fact(DisplayName = "UpdatePartColumns")]
		public async Task UpdatePartColumns()
		{
			using (var conn = CreateConnection())
			{
				// 如果实体的 Schema 没有配置表名，则使用类名
				await conn.ExecuteAsync(
					$"drop table if exists {Escape}test{Escape}.{Escape}updatepartcolumns{Escape};");
				var services = SpiderProvider.Value.CreateScopeServiceProvider();
				var storage = CreateStorage(StorageType.InsertIgnoreDuplicate);
				var dfc = new DataFlowContext(null, services);
				var typeName = typeof(CreateTableEntity6).FullName;
				var entity = new CreateTableEntity6();
				dfc.Add(typeName, entity.GetTableMetadata());
				var items = new ParseResult<CreateTableEntity6>
				{
					entity
				};
				dfc.AddParseItem(typeName, items);
				await storage.HandleAsync(dfc);

				var dfc2 = new DataFlowContext(null, services);
				dfc2.Add(typeName, entity.GetTableMetadata());
				var now = DateTime.Now;
				dfc2.AddParseItem(typeName, new ParseResult<CreateTableEntity6>
				{
					new CreateTableEntity6
					{
						Str1 = "TTT",
						DateTime = now,
						DateTimeOffset = now,
						Double = 888,
						Float = 999F,
						Required = 888
					}
				});
				var storage2 = CreateStorage(StorageType.Update);
				await storage2.HandleAsync(dfc2);

				var list = (await conn.QueryAsync<CreateTableEntity6>(
						$"SELECT * FROM {Escape}test{Escape}.{Escape}updatepartcolumns{Escape}"))
					.ToList();
				Assert.Single(list);
				entity = list.First();
				Assert.Equal("TTT", entity.Str1);
				Assert.Equal("yyy", entity.Str2);
				Assert.Equal(655, entity.Required);
				Assert.Equal(0, entity.Decimal);
				Assert.Equal(600, entity.Long);
				Assert.Equal(888, entity.Double);
				Assert.Equal(200.0F, entity.Float);

				await conn.ExecuteAsync(
					$"drop table if exists {Escape}test{Escape}.{Escape}updatepartcolumns{Escape};");
			}
		}

		/// <summary>
		/// 测试事务能否正常开启
		/// </summary>
		[Fact(DisplayName = "UseTransaction")]
		public async Task UseTransaction()
		{
			using (var conn = CreateConnection())
			{
				// 如果实体的 Schema 没有配置表名，则使用类名
				await conn.ExecuteAsync(
					$"drop table if exists {Escape}test{Escape}.{Escape}createtableprimay{Escape};");
				var services = SpiderProvider.Value.CreateScopeServiceProvider();
				var storage = (RelationalDatabaseEntityStorageBase) CreateStorage(StorageType.InsertIgnoreDuplicate);
				storage.UseTransaction = true;
				var dfc = new DataFlowContext(null, services);
				var typeName = typeof(CreateTableEntity4).FullName;
				var entity = new CreateTableEntity4();
				dfc.Add(typeName, entity.GetTableMetadata());
				var items = new ParseResult<CreateTableEntity4>
				{
					entity,
					entity,
					entity
				};
				dfc.AddParseItem(typeName, items);
				await storage.HandleAsync(dfc);
				var list = (await conn.QueryAsync<CreateTableEntity4>(
						$"SELECT * FROM {Escape}test{Escape}.{Escape}createtableprimay{Escape}"))
					.ToList();
				Assert.Single(list);
				entity = list.First();
				Assert.Equal("xxx", entity.Str1);
				Assert.Equal("yyy", entity.Str2);
				Assert.Equal(655, entity.Required);
				Assert.Equal(0, entity.Decimal);
				Assert.Equal(600, entity.Long);
				Assert.Equal(400, entity.Double);
				Assert.Equal(200.0F, entity.Float);

				await conn.ExecuteAsync(
					$"drop table if exists {Escape}test{Escape}.{Escape}createtableprimay{Escape};");
			}
		}

		/// <summary>
		/// 测试数据库名，表名，列名是的大小写是否正确
		/// </summary>
		[Fact(DisplayName = "IgnoreCase")]
		public async Task IgnoreCase()
		{
			using (var conn = CreateConnection())
			{
				// 如果实体的 Schema 没有配置表名，则使用类名
				await conn.ExecuteAsync($"drop table if exists {Escape}test{Escape}.{Escape}IgnoreCase{Escape};");
				var services = SpiderProvider.Value.CreateScopeServiceProvider();
				var storage = (RelationalDatabaseEntityStorageBase) CreateStorage(StorageType.Insert);
				storage.IgnoreCase = false;
				var dfc = new DataFlowContext(null, services);
				var typeName = typeof(CreateTableEntity7).FullName;
				var entity = new CreateTableEntity7();
				dfc.Add(typeName, entity.GetTableMetadata());
				var items = new ParseResult<CreateTableEntity7>
				{
					entity
				};
				dfc.AddParseItem(typeName, items);
				await storage.HandleAsync(dfc);
				var list = (await conn.QueryAsync<CreateTableEntity7>(
					$"SELECT * FROM {Escape}test{Escape}.{Escape}IgnoreCase{Escape}")).ToList();
				Assert.Single(list);
				entity = list.First();
				Assert.Equal("xxx", entity.Str1);
				Assert.Equal("yyy", entity.Str2);
				Assert.Equal(655, entity.Required);
				Assert.Equal(0, entity.Decimal);
				Assert.Equal(600, entity.Long);
				Assert.Equal(400, entity.Double);
				Assert.Equal(200.0F, entity.Float);
				await conn.ExecuteAsync($"drop table if exists {Escape}test{Escape}.{Escape}IgnoreCase{Escape};");
			}
		}

		[Fact(DisplayName = "Indexes")]
		public async Task Indexes()
		{
			using (var conn = CreateConnection())
			{
				await conn.ExecuteAsync(
					$"drop table if exists {Escape}test{Escape}.{Escape}createtableindexes{Escape};");
				var services = SpiderProvider.Value.CreateScopeServiceProvider();
				var storage = CreateStorage(StorageType.Insert);

				var dfc = new DataFlowContext(null, services);
				var typeName = typeof(CreateTableEntity5).FullName;
				var entity = new CreateTableEntity9();
				dfc.Add(typeName, entity.GetTableMetadata());
				var items = new ParseResult<CreateTableEntity9>
				{
					entity
				};
				dfc.AddParseItem(typeName, items);
				await storage.HandleAsync(dfc);
				var indexes = (await conn.QueryAsync<IndexInfo>
						($"show index from test.createtableindexes")
					).ToList();
				Assert.Equal(6, indexes.Count);
				Assert.Contains(indexes,
					x => x.Key_name == "INDEX_STR1" && x.Non_unique == 1 && x.Column_name == "str1");
				Assert.Contains(indexes, x =>
					x.Key_name == "INDEX_STR1_STR2" && x.Non_unique == 1 && x.Column_name == "str1");
				Assert.Contains(indexes, x =>
					x.Key_name == "INDEX_STR1_STR2" && x.Non_unique == 1 && x.Column_name == "str2");
				Assert.Contains(indexes,
					x => x.Key_name == "UNIQUE_STR3" && x.Non_unique == 0 && x.Column_name == "str3");
				Assert.Contains(indexes, x =>
					x.Key_name == "UNIQUE_STR3_STR4" && x.Non_unique == 0 && x.Column_name == "str3");
				Assert.Contains(indexes, x =>
					x.Key_name == "UNIQUE_STR3_STR4" && x.Non_unique == 0 && x.Column_name == "str4");

				await conn.ExecuteAsync(
					$"drop table if exists {Escape}test{Escape}.{Escape}createtableindexes{Escape};");
			}
		}

		class CreateTableEntity1 : EntityBase<CreateTableEntity1>
		{
			public string Str1 { get; set; } = "xxx";

			[StringLength(100)] public string Str2 { get; set; } = "yyy";

			[Required] public int Required { get; set; } = 655;

			public decimal Decimal { get; set; }

			public long Long { get; set; } = 600;

			public double Double { get; set; } = 400;

			public DateTime DateTime { get; set; } = DateTime.Now;

			public DateTimeOffset DateTimeOffset { get; set; } = DateTimeOffset.Now;

			public float Float { get; set; } = 200.0F;
		}

		[Schema(null, "CreateTableNoTableName")]
		class CreateTableEntity2 : EntityBase<CreateTableEntity2>
		{
			public string Str1 { get; set; } = "xxx";

			[StringLength(100)] public string Str2 { get; set; } = "yyy";

			[Required] public int Required { get; set; } = 655;

			public decimal Decimal { get; set; }

			public long Long { get; set; } = 600;

			public double Double { get; set; } = 400;

			public DateTime DateTime { get; set; } = DateTime.Now;

			public DateTimeOffset DateTimeOffset { get; set; } = DateTimeOffset.Now;

			public float Float { get; set; } = 200.0F;
		}

		[Schema("test", "CreateTable")]
		class CreateTableEntity3 : EntityBase<CreateTableEntity3>
		{
			public string Str1 { get; set; } = "xxx";

			[StringLength(100)] public string Str2 { get; set; } = "yyy";

			[Required] public int Required { get; set; } = 655;

			public decimal Decimal { get; set; }

			public long Long { get; set; } = 600;

			public double Double { get; set; } = 400;

			public DateTime DateTime { get; set; } = DateTime.Now;

			public DateTimeOffset DateTimeOffset { get; set; } = DateTimeOffset.Now;

			public float Float { get; set; } = 200.0F;
		}

		[Schema("test", "createtableprimay")]
		class CreateTableEntity4 : EntityBase<CreateTableEntity4>
		{
			public string Str1 { get; set; } = "xxx";

			[StringLength(100)] public string Str2 { get; set; } = "yyy";

			[Required] public int Required { get; set; } = 655;

			public decimal Decimal { get; set; }

			public long Long { get; set; } = 600;

			public double Double { get; set; } = 400;

			public DateTime DateTime { get; set; } = DateTime.Now;

			public DateTimeOffset DateTimeOffset { get; set; } = DateTimeOffset.Now;

			public float Float { get; set; } = 200.0F;

			protected override void Configure()
			{
				HasKey(x => x.Str2);
			}
		}

		[Schema("test", "createtableautoincprimay")]
		class CreateTableEntity5 : EntityBase<CreateTableEntity5>
		{
			public string Str1 { get; set; } = "xxx";

			[StringLength(100)] public string Str2 { get; set; } = "yyy";

			[Required] public int Required { get; set; } = 655;

			public decimal Decimal { get; set; }

			public long Long { get; set; } = 600;

			public double Double { get; set; } = 400;

			public DateTime DateTime { get; set; } = DateTime.Now;

			public DateTimeOffset DateTimeOffset { get; set; } = DateTimeOffset.Now;

			public float Float { get; set; } = 200.0F;

			public int Id { get; set; }
		}

		[Schema("test", "updatepartcolumns")]
		class CreateTableEntity6 : EntityBase<CreateTableEntity6>
		{
			public string Str1 { get; set; } = "xxx";

			[StringLength(100)] public string Str2 { get; set; } = "yyy";

			[Required] public int Required { get; set; } = 655;

			public decimal Decimal { get; set; }

			public long Long { get; set; } = 600;

			public double Double { get; set; } = 400;

			public DateTime DateTime { get; set; } = DateTime.Now;

			public DateTimeOffset DateTimeOffset { get; set; } = DateTimeOffset.Now;

			public float Float { get; set; } = 200.0F;

			protected override void Configure()
			{
				HasKey(x => x.Str2);
				ConfigureUpdateColumns(x => new {x.Str1, x.Double});
			}
		}

		[Schema("test", "IgnoreCase")]
		class CreateTableEntity7 : EntityBase<CreateTableEntity7>
		{
			public string Str1 { get; set; } = "xxx";

			[StringLength(100)] public string Str2 { get; set; } = "yyy";

			[Required] public int Required { get; set; } = 655;

			public decimal Decimal { get; set; }

			public long Long { get; set; } = 600;

			public double Double { get; set; } = 400;

			public DateTime DateTime { get; set; } = DateTime.Now;

			public DateTimeOffset DateTimeOffset { get; set; } = DateTimeOffset.Now;

			public float Float { get; set; } = 200.0F;
		}

		[Schema("test", "createtablemultiprimay")]
		class CreateTableEntity8 : EntityBase<CreateTableEntity8>
		{
			public string Str1 { get; set; } = "xxx";

			[StringLength(100)] public string Str2 { get; set; } = "yyy";

			[Required] public int Required { get; set; } = 655;

			public decimal Decimal { get; set; }

			public long Long { get; set; } = 600;

			public double Double { get; set; } = 400;

			public DateTime DateTime { get; set; } = DateTime.Now;

			public DateTimeOffset DateTimeOffset { get; set; } = DateTimeOffset.Now;

			public float Float { get; set; } = 200.0F;

			protected override void Configure()
			{
				HasKey(x => new {x.Str2, x.Decimal});
			}
		}

		[Schema("test", "createtableindexes")]
		class CreateTableEntity9 : EntityBase<CreateTableEntity9>
		{
			[StringLength(100)] public string Str1 { get; set; } = "Str1";

			[StringLength(100)] public string Str2 { get; set; } = "Str2";

			[StringLength(100)] public string Str3 { get; set; } = "Str3";

			[StringLength(100)] public string Str4 { get; set; } = "Str4";


			protected override void Configure()
			{
				HasIndex(x => x.Str1);
				HasIndex(x => new {x.Str1, x.Str2});

				HasIndex(x => x.Str3, true);
				HasIndex(x => new {x.Str3, x.Str4}, true);
			}
		}
	}
}
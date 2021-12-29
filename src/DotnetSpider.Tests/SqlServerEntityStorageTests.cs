using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DotnetSpider.DataFlow;
using DotnetSpider.DataFlow.Storage;
using Xunit;

namespace DotnetSpider.Tests
{
    public class SqlServerEntityStorageTests : TestBase
    {
        class IndexInfo
        {
            public int KEY_SEQ { get; set; }
            public string COLUMN_NAME { get; set; }
        }

        private readonly string _connectionString =
            "Data Source=.;Initial Catalog=master;User Id=sa;Password='1qazZAQ!'";

        protected IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        protected virtual EntityStorageBase CreateStorage(StorageMode type)
        {
            return new SqlServerEntityStorage(type, _connectionString);
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

        /// <summary>
        /// 测试能正确创建 MySql 表
        /// 1. 如果实体的 Schema 没有配置表名，则使用类名
        /// 2. 如果实体的 Schema 配置了表名，则使用配置的表名
        /// 3. 是否有正确添加表的后缀
        /// </summary>
        [Fact(DisplayName = "CreateTableNoSchema")]
        public async Task CreateTableNoSchema()
        {
	        using var conn = CreateConnection();
	        // 如果实体的 Schema 没有配置表名，则使用类名
	        await conn.ExecuteAsync("drop table if exists createtableentity1;");

	        {
		        var storage = CreateStorage(StorageMode.Insert);
		        var dfc = new DataFlowContext(null, null, null, null);
		        var typeName = typeof(CreateTableEntity1);
		        var entity = new CreateTableEntity1();

		        var items = new List<CreateTableEntity1> {entity};
		        dfc.AddData(typeName, items);
		        await storage.HandleAsync(dfc);
		        var list = (await conn.QueryAsync<CreateTableEntity1>("SELECT * FROM createtableentity1")).ToList();
		        Assert.Single(list);
		        entity = list.First();
		        Assert.Equal("xxx", entity.Str1);
		        Assert.Equal("yyy", entity.Str2);
		        Assert.Equal(655, entity.Required);
		        Assert.Equal(0, entity.Decimal);
		        Assert.Equal(600, entity.Long);
		        Assert.Equal(400, entity.Double);
		        Assert.Equal(200.0F, entity.Float);
		        await conn.ExecuteAsync("drop table if exists createtableentity1;");
	        }
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

        [Fact(DisplayName = "CreateTableNoTableName")]
        public async Task CreateTableNoTableName()
        {
	        using var conn = CreateConnection();
	        // 如果实体的 Schema 没有配置表名，则使用类名
	        await conn.ExecuteAsync("drop table if exists createtablenotablename;");

	        {
		        var storage = CreateStorage(StorageMode.Insert);
		        var dfc = new DataFlowContext(null, null, null, null);
		        var typeName = typeof(CreateTableEntity2);
		        var entity = new CreateTableEntity2();

		        var items = new List<CreateTableEntity2> {entity};
		        dfc.AddData(typeName, items);
		        await storage.HandleAsync(dfc);
		        var list = (await conn.QueryAsync<CreateTableEntity2>("SELECT * FROM createtablenotablename"))
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
		        await conn.ExecuteAsync("drop table if exists createtablenotablename;");
	        }
        }

        [Schema("test", "createtable")]
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

        [Fact(DisplayName = "CreateTable")]
        public async Task CreateTable()
        {
	        using var conn = CreateConnection();
	        // 如果实体的 Schema 没有配置表名，则使用类名
	        await conn.ExecuteAsync("drop table if exists test.dbo.createtable;");

	        {
		        var storage = CreateStorage(StorageMode.Insert);
		        var dfc = new DataFlowContext(null, null, null,null);
		        var typeName = typeof(CreateTableEntity3);
		        var entity = new CreateTableEntity3();

		        var items = new List<CreateTableEntity3> {entity};
		        dfc.AddData(typeName, items);
		        await storage.HandleAsync(dfc);
		        var list =
			        (await conn.QueryAsync<CreateTableEntity3>("SELECT * FROM test.dbo.createtable")).ToList();
		        Assert.Single(list);
		        entity = list.First();
		        Assert.Equal("xxx", entity.Str1);
		        Assert.Equal("yyy", entity.Str2);
		        Assert.Equal(655, entity.Required);
		        Assert.Equal(0, entity.Decimal);
		        Assert.Equal(600, entity.Long);
		        Assert.Equal(400, entity.Double);
		        Assert.Equal(200.0F, entity.Float);
		        await conn.ExecuteAsync("drop table if exists test.dbo.createtable;");
	        }
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

        [Fact(DisplayName = "CreateTablePrimary")]
        public async Task CreateTablePrimary()
        {
	        using var conn = CreateConnection();
	        // 如果实体的 Schema 没有配置表名，则使用类名
	        await conn.ExecuteAsync("drop table if exists test.dbo.createtableprimay;");

	        {
		        var storage = CreateStorage(StorageMode.Insert);
		        var dfc = new DataFlowContext(null, null, null, null);
		        var typeName = typeof(CreateTableEntity4);
		        var entity = new CreateTableEntity4();

		        var items = new List<CreateTableEntity4> {entity};
		        dfc.AddData(typeName, items);
		        await storage.HandleAsync(dfc);
		        var list = (await conn.QueryAsync<CreateTableEntity4>("SELECT * FROM test.dbo.createtableprimay"))
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

		        var primaries = (await conn.QueryAsync<IndexInfo>
				        (@"USE test; EXEC sp_pkeys @table_name='createtableprimay'")
			        ).ToList();
		        Assert.Single(primaries);
		        Assert.Equal("str2", primaries[0].COLUMN_NAME);
		        await conn.ExecuteAsync("drop table if exists test.dbo.createtableprimay;");
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

        [Fact(DisplayName = "CreateTableAutoIncPrimary")]
        public async Task CreateTableAutoIncPrimary()
        {
	        using var conn = CreateConnection();
	        // 如果实体的 Schema 没有配置表名，则使用类名
	        await conn.ExecuteAsync("drop table if exists test.dbo.createtableautoincprimay;");

	        {
		        var storage = CreateStorage(StorageMode.Insert);
		        var dfc = new DataFlowContext(null, null, null, null);
		        var typeName = typeof(CreateTableEntity5);
		        var entity = new CreateTableEntity5();

		        var items = new List<CreateTableEntity5> {entity, entity};
		        dfc.AddData(typeName, items);
		        await storage.HandleAsync(dfc);
		        var list =
			        (await conn.QueryAsync<CreateTableEntity5>("SELECT * FROM test.dbo.createtableautoincprimay"))
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

		        var primaries = (await conn.QueryAsync<IndexInfo>
				        (@"USE test; EXEC sp_pkeys @table_name='createtableautoincprimay'")
			        ).ToList();
		        Assert.Single(primaries);
		        Assert.Equal("id", primaries[0].COLUMN_NAME);
		        Assert.Equal(1, primaries[0].KEY_SEQ);
		        await conn.ExecuteAsync("drop table if exists test.dbo.createtableautoincprimay;");
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
	        using var conn = CreateConnection();
	        // 如果实体的 Schema 没有配置表名，则使用类名
	        await conn.ExecuteAsync("drop table if exists test.dbo.createtableprimay;");

	        {
		        var storage = CreateStorage(StorageMode.InsertIgnoreDuplicate);
		        var dfc = new DataFlowContext(null, null, null, null);
		        var typeName = typeof(CreateTableEntity4);
		        var entity = new CreateTableEntity4();

		        var items = new List<CreateTableEntity4> {entity, entity, entity};
		        dfc.AddData(typeName, items);
		        await storage.HandleAsync(dfc);
		        var list = (await conn.QueryAsync<CreateTableEntity4>("SELECT * FROM test.dbo.createtableprimay"))
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

		        await conn.ExecuteAsync("drop table if exists test.dbo.createtableprimay;");
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
	        using var conn = CreateConnection();
	        // 如果实体的 Schema 没有配置表名，则使用类名
	        await conn.ExecuteAsync("drop table if exists test.dbo.createtableprimay;");

	        {
		        var storage = CreateStorage(StorageMode.InsertAndUpdate);
		        var dfc = new DataFlowContext(null, null, null, null);
		        var typeName = typeof(CreateTableEntity4);
		        var entity = new CreateTableEntity4();

		        var items = new List<CreateTableEntity4> {entity, new() {Str1 = "zzz"}};
		        dfc.AddData(typeName, items);
		        await storage.HandleAsync(dfc);
		        var list = (await conn.QueryAsync<CreateTableEntity4>("SELECT * FROM test.dbo.createtableprimay"))
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

		        await conn.ExecuteAsync("drop table if exists test.dbo.createtableprimay;");
	        }
        }

        /// <summary>
        /// 测试能否正确更新数据
        /// </summary>
        [Fact(DisplayName = "UpdateAllColumns")]
        public async Task UpdateAllColumns()
        {
	        using var conn = CreateConnection();
	        // 如果实体的 Schema 没有配置表名，则使用类名
	        await conn.ExecuteAsync("drop table if exists test.dbo.createtableprimay;");

	        {
		        var storage = CreateStorage(StorageMode.InsertIgnoreDuplicate);
		        var dfc = new DataFlowContext(null, null, null, null);
		        var typeName = typeof(CreateTableEntity4);
		        var entity = new CreateTableEntity4();

		        var items = new List<CreateTableEntity4> {entity};
		        dfc.AddData(typeName, items);
		        await storage.HandleAsync(dfc);

		        var dfc2 = new DataFlowContext(null, null, null, null);

		        var now = DateTime.Now;
		        dfc2.AddData(typeName,
			        new List<CreateTableEntity4>
			        {
				        new()
				        {
					        Str1 = "TTT", DateTime = now, DateTimeOffset = now, Double = 888
				        }
			        });
		        var storage2 = CreateStorage(StorageMode.Update);
		        await storage2.HandleAsync(dfc2);

		        var list = (await conn.QueryAsync<CreateTableEntity4>("SELECT * FROM test.dbo.createtableprimay"))
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

		        await conn.ExecuteAsync("drop table if exists test.dbo.createtableprimay;");
	        }
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

        /// <summary>
        /// 测试能否正确更新数据
        /// </summary>
        [Fact(DisplayName = "UpdatePartColumns")]
        public async Task UpdatePartColumns()
        {
	        using var conn = CreateConnection();
	        // 如果实体的 Schema 没有配置表名，则使用类名
	        await conn.ExecuteAsync("drop table if exists test.dbo.updatepartcolumns;");

	        {
		        var storage = CreateStorage(StorageMode.InsertIgnoreDuplicate);
		        var dfc = new DataFlowContext(null, null, null, null);
		        var typeName = typeof(CreateTableEntity6);
		        var entity = new CreateTableEntity6();

		        var items = new List<CreateTableEntity6> {entity};
		        dfc.AddData(typeName, items);
		        await storage.HandleAsync(dfc);

		        var dfc2 = new DataFlowContext(null, null, null, null);
		        var now = DateTime.Now;
		        dfc2.AddData(typeName,
			        new List<CreateTableEntity6>
			        {
				        new()
				        {
					        Str1 = "TTT",
					        DateTime = now,
					        DateTimeOffset = now,
					        Double = 888,
					        Float = 999F,
					        Required = 888
				        }
			        });
		        var storage2 = CreateStorage(StorageMode.Update);
		        await storage2.HandleAsync(dfc2);

		        var list = (await conn.QueryAsync<CreateTableEntity6>("SELECT * FROM test.dbo.updatepartcolumns"))
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

		        await conn.ExecuteAsync("drop table if exists test.dbo.updatepartcolumns;");
	        }
        }

        /// <summary>
        /// 测试事务能否正常开启
        /// </summary>
        [Fact(DisplayName = "UseTransaction")]
        public async Task UseTransaction()
        {
	        using var conn = CreateConnection();
	        // 如果实体的 Schema 没有配置表名，则使用类名
	        await conn.ExecuteAsync("drop table if exists test.dbo.createtableprimay;");

	        {
		        var storage =
			        (RelationalDatabaseEntityStorageBase) CreateStorage(StorageMode.InsertIgnoreDuplicate);
		        storage.UseTransaction = true;
		        var dfc = new DataFlowContext(null, null, null, null);
		        var typeName = typeof(CreateTableEntity4);
		        var entity = new CreateTableEntity4();

		        var items = new List<CreateTableEntity4> {entity, entity, entity};
		        dfc.AddData(typeName, items);
		        await storage.HandleAsync(dfc);
		        var list = (await conn.QueryAsync<CreateTableEntity4>("SELECT * FROM test.dbo.createtableprimay"))
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

		        await conn.ExecuteAsync("drop table if exists test.dbo.createtableprimay;");
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

        /// <summary>
        /// 测试数据库名，表名，列名是的大小写是否正确
        /// </summary>
        [Fact(DisplayName = "IgnoreCase")]
        public async Task IgnoreCase()
        {
	        using var conn = CreateConnection();
	        // 如果实体的 Schema 没有配置表名，则使用类名
	        await conn.ExecuteAsync("drop table if exists test.dbo.IgnoreCase;");

	        {
		        var storage = (RelationalDatabaseEntityStorageBase) CreateStorage(StorageMode.Insert);
		        storage.IgnoreCase = false;
		        var dfc = new DataFlowContext(null, null, null, null);
		        var typeName = typeof(CreateTableEntity7);
		        var entity = new CreateTableEntity7();

		        var items = new List<CreateTableEntity7> {entity};
		        dfc.AddData(typeName, items);
		        await storage.HandleAsync(dfc);
		        var list = (await conn.QueryAsync<CreateTableEntity7>("SELECT * FROM test.dbo.IgnoreCase"))
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
		        await conn.ExecuteAsync("drop table if exists test.dbo.IgnoreCase;");
	        }
        }
    }
}

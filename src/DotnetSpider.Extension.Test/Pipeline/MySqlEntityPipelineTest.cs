using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using DotnetSpider.Core;
using DotnetSpider.Extension.Pipeline;
using MySql.Data.MySqlClient;
using Xunit;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Downloader;
using DotnetSpider.Extension.Model;

namespace DotnetSpider.Extension.Test.Pipeline
{
	/// <summary>
	/// grant all privileges on *.* to root@localhost identified by '';
	/// flush privileges;
	/// 
	/// 测试流程:
	/// 1. 构造 Pipeline 对象
	/// 2. 构造 ModelDefine
	/// 3. 构造解析好的数据
	/// 4. Process Pipeline
	/// </summary>
	public class MySqlEntityPipelineTest : TestBase
	{
		public MySqlEntityPipelineTest()
		{
			Env.HubService = false;
		}

		protected virtual DbEntityPipeline CreatePipeline(PipelineMode pipelineMode = PipelineMode.InsertAndIgnoreDuplicate)
		{
			return new MySqlEntityPipeline(DefaultConnectionString, pipelineMode);
		}

		protected virtual IDbConnection CreateDbConnection()
		{
			return new MySqlConnection(DefaultConnectionString);
		}

		[Fact(DisplayName = "DataTypes")]
		public virtual void DataTypes()
		{
			using (MySqlConnection conn = new MySqlConnection("Database='mysql';Data Source=127.0.0.1;User ID=root;Password=1qazZAQ!;Port=3306;SslMode=None;"))
			{
				try
				{
					conn.Execute("use test;  drop table table15;");
				}
				catch
				{
				}

				var pipeline = new MySqlEntityPipeline("Database='mysql';Data Source=localhost;User ID=root;Password=1qazZAQ!;Port=3306;SslMode=None;");
				var resultItems = new ResultItems();
				resultItems.Request = new Request();
				resultItems["aaaa"] =
					new List<Entity15> {
						new Entity15
					{
						 Int=1,
						 Bool=true,
						 BigInt=11,
						 String="aaa",
						 Time=new DateTime(2018,06,12),
						 Float=1,
						 Double=1,
						 String1="abc",
						 String2="abcdd",
						 Decimal=1
					}
					};
				pipeline.Process(new ResultItems[] { resultItems }, null);

				var columns = conn.Query<ColumnInfo>("SELECT COLUMN_NAME as `Name`, COLUMN_TYPE as `Type` FROM information_schema.columns WHERE table_name='table15' AND table_schema = 'test';").ToList(); ;
				Assert.Equal(12, columns.Count);

				Assert.Equal("int".ToLower(), columns[0].Name);
				Assert.Equal("bool".ToLower(), columns[1].Name);
				Assert.Equal("bigint".ToLower(), columns[2].Name);
				Assert.Equal("string".ToLower(), columns[3].Name);
				Assert.Equal("time".ToLower(), columns[4].Name);
				Assert.Equal("float".ToLower(), columns[5].Name);
				Assert.Equal("double".ToLower(), columns[6].Name);
				Assert.Equal("string1".ToLower(), columns[7].Name);
				Assert.Equal("string2".ToLower(), columns[8].Name);
				Assert.Equal("decimal".ToLower(), columns[9].Name);
				Assert.Equal("creation_time".ToLower(), columns[10].Name);
				Assert.Equal("creation_date".ToLower(), columns[11].Name);


				Assert.Equal("int(11)", columns[0].Type);
				Assert.Equal("tinyint(1)", columns[1].Type);
				Assert.Equal("bigint(20)", columns[2].Type);
				Assert.Equal("varchar(255)", columns[3].Type);
				Assert.Equal("timestamp", columns[4].Type);
				Assert.Equal("float", columns[5].Type);
				Assert.Equal("double", columns[6].Type);
				Assert.Equal("varchar(100)", columns[7].Type);
				Assert.Equal("longtext", columns[8].Type);
				Assert.Equal("decimal(18,2)", columns[9].Type);
				Assert.Equal("timestamp", columns[10].Type);
				Assert.Equal("date", columns[11].Type);


				try
				{
					conn.Execute("use test;  drop table table15;");
				}
				catch
				{
				}
			}
		}

		[Fact(DisplayName = "Update_AutoIncrementPrimaryKey")]
		public virtual void Update_AutoIncrementPrimaryKey()
		{
			using (var conn = CreateDbConnection())
			{
				try
				{
					conn.Execute("use test; DROP TABLE autoincrementprimarykey;");
				}
				catch { }

				ISpider spider = new DefaultSpider("test");

				// 1. Create pipeline
				var pipeline = CreatePipeline();

				// 2. Create ModelDefine
				var metadata = new ModelDefinition<AutoIncrementPrimaryKey>();

				// 3. Create data
				var resultItems = new ResultItems();
				resultItems.Request = new Request();
				resultItems["AAA"] = new List<AutoIncrementPrimaryKey>
				{
					new AutoIncrementPrimaryKey { Sku = "110", Category = "3C", Name = "Product 1" },
					new AutoIncrementPrimaryKey { Sku = "111", Category = "3C", Name = "Product 2" }
				};
				var processArgument = new ResultItems[] { resultItems };

				// 4. Execute pipline
				pipeline.Process(processArgument, spider);

				var updateModePipeline = CreatePipeline(PipelineMode.Update);

				resultItems = new ResultItems();
				resultItems.Request = new Request();
				resultItems["AAA"] = new List<AutoIncrementPrimaryKey>
				{
					 new AutoIncrementPrimaryKey { Id = 1, Category = "4C" }
				};

				updateModePipeline.Process(new ResultItems[] { resultItems }, spider);

				var list = conn.Query<AutoIncrementPrimaryKey>("use test; select * from autoincrementprimarykey").ToList();
				Assert.Equal(2, list.Count);
				Assert.Equal("110", list[0].Sku);
				Assert.Equal("4C", list[0].Category);

				try
				{
					conn.Execute("use test; DROP TABLE autoincrementprimarykey;");
				}
				catch { }
			}
		}

		[Fact(DisplayName = "Update_MutliPrimaryKey")]
		public virtual void Update_MutliPrimaryKey()
		{
			using (var conn = CreateDbConnection())
			{
				try
				{
					conn.Execute("use test; DROP TABLE multiprimarykey;");
				}
				catch { }
				ISpider spider = new DefaultSpider("test");

				// 1. Create pipeline
				var pipeline = CreatePipeline();

				// 2. Create ModelDefine
				var metadata = new ModelDefinition<MultiPrimaryKey>();

				// 3. Create data
				var resultItems = new ResultItems();
				resultItems.Request = new Request();
				resultItems["AAA"] = new List<MultiPrimaryKey>
				{
					new MultiPrimaryKey { Sku = "110", Category = "3C", Name = "Product 1" },
					new MultiPrimaryKey { Sku = "111", Category = "3C", Name = "Product 2" }
				};
				var processArgument = new ResultItems[] { resultItems };

				// 4. Execute pipline
				pipeline.Process(processArgument, spider);

				var updateModePipeline = CreatePipeline(PipelineMode.Update);

				resultItems = new ResultItems();
				resultItems.Request = new Request();
				resultItems["AAA"] = new List<MultiPrimaryKey>
				{
					 new MultiPrimaryKey { Sku="111", Category = "4C", Name="Product 2" }
				};

				updateModePipeline.Process(new ResultItems[] { resultItems }, spider);

				var list = conn.Query<MultiPrimaryKey>("use test; select * from multiprimarykey").ToList();
				Assert.Equal(2, list.Count);
				Assert.Equal("4C", list[1].Category);
				try
				{
					conn.Execute("use test; DROP TABLE multiprimarykey;");
				}
				catch { }
			}
		}

		#region Insert Tests

		[Fact(DisplayName = "Insert_AutoIncrementPrimaryKey")]
		public virtual void Insert_AutoIncrementPrimaryKey()
		{
			using (var conn = CreateDbConnection())
			{
				try
				{
					conn.Execute("use test; DROP TABLE autoincrementprimarykey;");
				}
				catch { }

				var pipeline = CreatePipeline();
				var metadata = new ModelDefinition<AutoIncrementPrimaryKey>();

				var resultItems = new ResultItems();
				resultItems.Request = new Request();

				resultItems["AAA"] = new List<AutoIncrementPrimaryKey>
				{
					new AutoIncrementPrimaryKey { Sku = "110", Category = "3C", Name = "Product 1" },
					new AutoIncrementPrimaryKey { Sku = "111", Category = "3C", Name = "Product 2" },
					new AutoIncrementPrimaryKey { Sku = "112", Category = null, Name = "Product 3" }
				};
				pipeline.Process(new ResultItems[] { resultItems }, null);

				var list = conn.Query("use test; select * from autoincrementprimarykey").Select(r => r as IDictionary<string, dynamic>).ToList();

				Assert.Equal(3, list.Count);
				Assert.Equal(1, list[0]["id"]);
				Assert.Equal(2, list[1]["id"]);
				Assert.Equal(3, list[2]["id"]);
				try
				{
					conn.Execute("use test; DROP TABLE autoincrementprimarykey;");
				}
				catch { }
			}
		}

		[Fact(DisplayName = "Insert_NonePrimaryKey")]
		public virtual void Insert_NonePrimaryKey()
		{
			using (var conn = CreateDbConnection())
			{
				try
				{
					conn.Execute("use test; DROP TABLE noneprimarykey;");
				}
				catch { }

				var pipeline = CreatePipeline();
				var metadata = new ModelDefinition<NonePrimaryKey>();

				var resultItems = new ResultItems();
				resultItems.Request = new Request();

				resultItems["AAA"] = new List<NonePrimaryKey>
				{
					new NonePrimaryKey { Sku = "110", Category = "3C", Name = "Product 1" },
					new NonePrimaryKey { Sku = "111", Category = "3C", Name = "Product 2" },
					new NonePrimaryKey { Sku = "112", Category = null, Name = "Product 3" },
					new NonePrimaryKey { Sku = "110", Category = "3C", Name = "Product 1" },
				};
				pipeline.Process(new ResultItems[] { resultItems }, null);

				var list = conn.Query<NonePrimaryKey>("use test; select * from noneprimarykey").ToList();

				Assert.Equal(4, list.Count);
				Assert.Equal("110", list[0].Sku);
				Assert.Equal("111", list[1].Sku);
				Assert.Null(list[2].Category);
				try
				{
					conn.Execute("use test; DROP TABLE noneprimarykey;");
				}
				catch { }
			}
		}

		[Fact(DisplayName = "Insert_AutoTimestamp")]
		public virtual void Insert_AutoTimestamp()
		{
			using (var conn = CreateDbConnection())
			{
				var datetime = DateTime.Now.AddSeconds(-50);

				try
				{
					conn.Execute("use test; DROP TABLE timestamp;");
				}
				catch { }

				var spider = new DefaultSpider();

				var pipeline = CreatePipeline();
				var metadata = new ModelDefinition<Timestamp>();

				var resultItems = new ResultItems();
				resultItems.Request = new Request();

				resultItems["AAA"] = new List<Timestamp>
				{
					new Timestamp { Sku = "110", Category = "3C", Name = "Product 1" },
					new Timestamp { Sku = "111", Category = "3C", Name = "Product 2" },
					new Timestamp { Sku = "112", Category = null, Name = "Product 3" }
				};
				pipeline.Process(new ResultItems[] { resultItems }, spider);

				var list = conn.Query("use test; select * from timestamp").Select(r => r as IDictionary<string, dynamic>).ToList();

				Assert.Equal(6, list[0].Count);
				Assert.Equal(DateTime.Now.Date, list[0]["creation_date"]);
				Assert.Equal(DateTime.Now.Date, list[1]["creation_date"]);
				Assert.Equal(DateTime.Now.Date, list[2]["creation_date"]);
				Assert.True(list[0]["creation_time"] > datetime);
				Assert.True(list[1]["creation_time"] > datetime);
				Assert.True(list[2]["creation_time"] > datetime);

				try
				{
					conn.Execute("use test; DROP TABLE timestamp;");
				}
				catch { }
			}
		}

		[Fact(DisplayName = "Insert_NoneTimestamp")]
		public virtual void Insert_NoneTimestamp()
		{
			using (var conn = CreateDbConnection())
			{
				var datetime = DateTime.Now.AddSeconds(-50);

				try
				{
					conn.Execute("use test; DROP TABLE timestamp;");
				}
				catch { }


				var pipeline = CreatePipeline();
				pipeline.AutoTimestamp = false;

				var metadata = new ModelDefinition<Timestamp>();

				var resultItems = new ResultItems();
				resultItems.Request = new Request();

				resultItems["AAA"] = new List<Timestamp>
				{
					new Timestamp { Sku = "110", Category = "3C", Name = "Product 1" },
					new Timestamp { Sku = "111", Category = "3C", Name = "Product 2" },
					new Timestamp { Sku = "112", Category = null, Name = "Product 3" }
				};
				pipeline.Process(new ResultItems[] { resultItems }, null);

				var list = conn.Query("use test; select * from timestamp").Select(r => r as IDictionary<string, dynamic>).ToList();

				Assert.Equal(4, list[0].Count);

				try
				{
					conn.Execute("use test; DROP TABLE timestamp;");
				}
				catch { }
			}
		}

		[Fact(DisplayName = "Insert_MultiPrimaryKey")]
		public virtual void Insert_MultiPrimaryKey()
		{
			using (var conn = CreateDbConnection())
			{
				try
				{
					conn.Execute("use test; DROP TABLE multiprimarykey;");
				}
				catch { }

				var spider = new DefaultSpider();

				var pipeline = CreatePipeline();
				var metadata = new ModelDefinition<MultiPrimaryKey>();

				var resultItems = new ResultItems();
				resultItems.Request = new Request();

				resultItems["AAA"] = new List<MultiPrimaryKey>
				{
					new MultiPrimaryKey { Sku = "110", Category = "3C", Name = "Product 1" },
					new MultiPrimaryKey { Sku = "111", Category = "3C", Name = "Product 2" },
					new MultiPrimaryKey { Sku = "112", Category = null, Name = "Product 3" },
					new MultiPrimaryKey { Sku = "110", Category = "3C", Name = "Product 1" },
				};
				pipeline.Process(new ResultItems[] { resultItems }, spider);

				var list = conn.Query("use test; select * from multiprimarykey").Select(r => r as IDictionary<string, dynamic>).ToList();

				Assert.Equal(3, list.Count);
				Assert.Equal("110", list[0]["sku"]);
				Assert.Equal("111", list[1]["sku"]);
				Assert.Null(list[2]["category"]);

				try
				{
					conn.Execute("use test; DROP TABLE multiprimarykey;");
				}
				catch { }
			}
		}

		[Fact(DisplayName = "Insert_InsertNewAndUpdateOld")]
		public virtual void Insert_InsertNewAndUpdateOld()
		{
			using (var conn = CreateDbConnection())
			{
				try
				{
					conn.Execute("use test; DROP TABLE multiprimarykey;");
				}
				catch
				{
				}

				var pipeline = CreatePipeline();
				var metadata = new ModelDefinition<MultiPrimaryKey>();

				var resultItems = new ResultItems();
				resultItems.Request = new Request();

				resultItems["AAA"] = new List<MultiPrimaryKey>
				{
					new MultiPrimaryKey { Sku = "110", Category = "3C", Name = "Product 1" },
					new MultiPrimaryKey { Sku = "111", Category = "3C", Name = "Product 2" },
					new MultiPrimaryKey { Sku = "112", Category = null, Name = "Product 3" },
					new MultiPrimaryKey { Sku = "110", Category = "3C", Name = "Product 1" },
				};
				pipeline.Process(new ResultItems[] { resultItems }, null);

				var insertNewAndUpdateOldPipeline = CreatePipeline(PipelineMode.InsertNewAndUpdateOld);

				resultItems = new ResultItems();
				resultItems.Request = new Request();
				resultItems["AAA"] = new List<MultiPrimaryKey>
				{
					 new MultiPrimaryKey { Sku = "110", Name="Product 1", Category = "4C" }
				};

				insertNewAndUpdateOldPipeline.Process(new ResultItems[] { resultItems }, null);

				var list = conn.Query("use test; select * from multiprimarykey").Select(r => r as IDictionary<string, dynamic>).ToList();

				Assert.Equal(3, list.Count);
				Assert.Equal("4C", list[0]["category"]);

				try
				{
					conn.Execute("use test; DROP TABLE multiprimarykey;");
				}
				catch { }
			}
		}

		#endregion

		[Schema("test", "multiprimarykey")]
		public class MultiPrimaryKey : IBaseEntity
		{
			[Update]
			[Column(Length = 50)]
			public string Category { get; set; }

			[Primary]
			[Column(Length = 50)]
			public string Name { get; set; }

			[Primary]
			[Column(Length = 50)]
			public string Sku { get; set; }
		}
		[Schema("test", "timestamp")]
		public class Timestamp : BaseEntity
		{
			[Column(Length = 100)]
			public string Category { get; set; }

			[Column]
			public string Name { get; set; }

			[Column(Length = 100)]
			public string Sku { get; set; }
		}

		[Schema("test", "autoincrementprimarykey")]
		public class AutoIncrementPrimaryKey : BaseEntity
		{
			[Update]
			[Column]
			public string Category { get; set; }

			[Column]
			public string Name { get; set; }

			[Column]
			public string Sku { get; set; }
		}

		[Schema("test", "noneprimarykey")]
		public class NonePrimaryKey : IBaseEntity
		{
			[Column(Length = 100)]
			public string Category { get; set; }

			[Column()]
			public string Name { get; set; }

			[Column(Length = 100)]
			public string Sku { get; set; }
		}

		private class ColumnInfo
		{
			public string Name { get; set; }
			public string Type { get; set; }

			public override string ToString()
			{
				return $"{Name} {Type}";
			}
		}

		[Schema("test", "table15")]
		private class Entity15 : IBaseEntity
		{
			[Column()]
			public int Int { get; set; }

			[Column()]
			public bool Bool { get; set; }

			[Column()]
			public long BigInt { get; set; }

			[Column()]
			public string String { get; set; }

			[Column()]
			public DateTime Time { get; set; }

			[Column()]
			public float Float { get; set; }

			[Column()]
			public double Double { get; set; }

			[Column(Length =100)]
			public string String1 { get; set; }

			[Column(Length = 0)]
			public string String2 { get; set; }

			[Column()]
			public decimal Decimal { get; set; }
		}
	}
}

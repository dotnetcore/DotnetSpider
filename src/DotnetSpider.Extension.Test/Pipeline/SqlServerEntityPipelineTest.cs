using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using DotnetSpider.Core;
using DotnetSpider.Extension.Pipeline;
using Xunit;
using Dapper;
using DotnetSpider.Extension.Processor;
using System.Data;
using DotnetSpider.Extraction.Model.Attribute;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Extension.Model;
#if NETSTANDARD
using System.Runtime.InteropServices;
#endif
using DotnetSpider.Downloader;

namespace DotnetSpider.Extension.Test.Pipeline
{
	/// <summary>
	/// CREATE database  test firstly
	/// </summary>
	public class SqlServerEntityPipelineTest : MySqlEntityPipelineTest
	{
		public override string DefaultConnectionString => "Data Source=.\\SQLEXPRESS;Initial Catalog=master;Integrated Security=True";

		[Fact(DisplayName = "DataTypes")]
		public override void DataTypes()
		{
			if (!Env.IsWindows)
			{
				return;
			}
			using (var conn = new SqlConnection("Server=.\\SQLEXPRESS;Database=master;Trusted_Connection=True;MultipleActiveResultSets=true"))
			{
				try
				{
					conn.Execute("create database test;");
				}
				catch
				{
				}
				try
				{
					conn.Execute("USE [test]; drop table [test].dbo.[table15]");
				}
				catch
				{
				}
				var pipeline = new SqlServerEntityPipeline("Server=.\\SQLEXPRESS;Database=master;Trusted_Connection=True;MultipleActiveResultSets=true");
				var resultItems = new ResultItems();
				resultItems.Request = new Request();
				resultItems["aaa"] = new List<Entity15>  {
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

				var columns = conn.Query<ColumnInfo>("USE [test];select  b.name Name,c.name+'(' + cast(c.length as varchar)+')' [Type] from sysobjects a,syscolumns b,systypes c where a.id=b.id and a.name='table15' and a.xtype='U'and b.xtype=c.xtype").ToList();
				Assert.Equal(15, columns.Count);

				Assert.Equal("creation_date".ToLower(), columns[0].Name);
				Assert.Equal("int".ToLower(), columns[1].Name);
				Assert.Equal("time".ToLower(), columns[2].Name);
				Assert.Equal("creation_time".ToLower(), columns[3].Name);
				Assert.Equal("float".ToLower(), columns[4].Name);
				Assert.Equal("double".ToLower(), columns[5].Name);
				Assert.Equal("bool".ToLower(), columns[6].Name);
				Assert.Equal("decimal".ToLower(), columns[7].Name);
				Assert.Equal("bigint".ToLower(), columns[8].Name);
				Assert.Equal("string".ToLower(), columns[9].Name);
				Assert.Equal("string1".ToLower(), columns[10].Name);
				Assert.Equal("string2".ToLower(), columns[11].Name);


				Assert.Equal("date(3)", columns[0].Type);
				Assert.Equal("int(4)", columns[1].Type);
				Assert.Equal("datetime(8)", columns[2].Type);
				Assert.Equal("datetime(8)", columns[3].Type);
				Assert.Equal("float(8)", columns[4].Type);
				Assert.Equal("float(8)", columns[5].Type);
				Assert.Equal("bit(1)", columns[6].Type);
				Assert.Equal("decimal(17)", columns[7].Type);
				Assert.Equal("bigint(8)", columns[8].Type);
				Assert.Equal("nvarchar(8000)", columns[9].Type);
				Assert.Equal("nvarchar(8000)", columns[10].Type);
				Assert.Equal("nvarchar(8000)", columns[11].Type);

				conn.Execute("USE [test]; drop table [test].dbo.[table15]");
			}
		}

		protected override IDbConnection CreateDbConnection()
		{
			return new SqlConnection(DefaultConnectionString);
		}

		protected override DbEntityPipeline CreatePipeline(PipelineMode pipelineMode = PipelineMode.InsertAndIgnoreDuplicate)
		{

			return new SqlServerEntityPipeline(DefaultConnectionString, pipelineMode);
		}

		public override void Insert_InsertNewAndUpdateOld()
		{
		}

		public override void Insert_AutoIncrementPrimaryKey()
		{
			if (!Env.IsWindows)
			{
				return;
			}
			base.Insert_AutoIncrementPrimaryKey();
		}

		public override void Insert_AutoTimestamp()
		{
			if (!Env.IsWindows)
			{
				return;
			}
			base.Insert_AutoTimestamp();
		}

		public override void Insert_MultiPrimaryKey()
		{
			if (!Env.IsWindows)
			{
				return;
			}
			base.Insert_MultiPrimaryKey();
		}

		public override void Insert_NonePrimaryKey()
		{
			if (!Env.IsWindows)
			{
				return;
			}
			base.Insert_NonePrimaryKey();
		}

		public override void Insert_NoneTimestamp()
		{
			if (!Env.IsWindows)
			{
				return;
			}
			base.Insert_NoneTimestamp();
		}

		public override void Update_AutoIncrementPrimaryKey()
		{
			if (!Env.IsWindows)
			{
				return;
			}
			base.Update_AutoIncrementPrimaryKey();
		}

		public override void Update_MutliPrimaryKey()
		{
			if (!Env.IsWindows)
			{
				return;
			}
			base.Update_MutliPrimaryKey();
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
			[Column]
			[Field(Expression = "Url")]
			public int Int { get; set; }

			[Column]
			[Field(Expression = "Url")]
			public bool Bool { get; set; }

			[Column]
			[Field(Expression = "Url")]
			public long BigInt { get; set; }

			[Column]
			[Field(Expression = "Url")]
			public string String { get; set; }

			[Column]
			[Field(Expression = "Url")]
			public DateTime Time { get; set; }

			[Column]
			[Field(Expression = "Url")]
			public float Float { get; set; }

			[Column]
			[Field(Expression = "Url")]
			public double Double { get; set; }

			[Field(Expression = "Url")]
			[Column(100)]
			public string String1 { get; set; }

			[Column(Length = 0)]
			[Field(Expression = "Url")]
			public string String2 { get; set; }

			[Column]
			[Field(Expression = "Url")]
			public decimal Decimal { get; set; }
		}
	}
}

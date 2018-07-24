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
using DotnetSpider.Common;
using DotnetSpider.Extraction.Model;
#if NETSTANDARD
using System.Runtime.InteropServices;
#endif

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


				var spider = new DefaultSpider();

				EntityProcessor<Entity15> processor = new EntityProcessor<Entity15>();

				var pipeline = new SqlServerEntityPipeline("Server=.\\SQLEXPRESS;Database=master;Trusted_Connection=True;MultipleActiveResultSets=true");
				var resultItems = new ResultItems();
				resultItems.Request = new Request();
				resultItems.AddOrUpdateResultItem(processor.Model.Identity, new Tuple<IModel, IList<dynamic>>(processor.Model, new dynamic[] {
					new Dictionary<string, dynamic>
					{
						{ "int", "1"},
						{ "bool", "1"},
						{ "bigint", "11"},
						{ "string", "aaa"},
						{ "time", "2018-06-12"},
						{ "float", "1"},
						{ "double", "1"},
						{ "string1", "abc"},
						{ "string2", "abcdd"},
						{ "decimal", "1"}
					}
				}));
				pipeline.Process(new ResultItems[] { resultItems }, spider.Logger, spider);

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

		protected override DbModelPipeline CreatePipeline(PipelineMode pipelineMode = PipelineMode.InsertAndIgnoreDuplicate)
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

		[TableInfo("test", "table15")]
		private class Entity15
		{
			[FieldSelector(Expression = "Url")]
			public int Int { get; set; }

			[FieldSelector(Expression = "Url")]
			public bool Bool { get; set; }

			[FieldSelector(Expression = "Url")]
			public long BigInt { get; set; }

			[FieldSelector(Expression = "Url")]
			public string String { get; set; }

			[FieldSelector(Expression = "Url")]
			public DateTime Time { get; set; }

			[FieldSelector(Expression = "Url")]
			public float Float { get; set; }

			[FieldSelector(Expression = "Url")]
			public double Double { get; set; }

			[FieldSelector(Expression = "Url", Length = 100)]
			public string String1 { get; set; }

			[FieldSelector(Expression = "Url", Length = 0)]
			public string String2 { get; set; }

			[FieldSelector(Expression = "Url")]
			public decimal Decimal { get; set; }
		}
	}
}

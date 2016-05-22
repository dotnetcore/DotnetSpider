using System;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Extension.ORM;
using Java2Dotnet.Spider.Extension.Pipeline;
using Newtonsoft.Json.Linq;

namespace Java2Dotnet.Spider.Extension.Configuration
{
	public abstract class Pipeline
	{
		[Flags]
		public enum Types
		{
			Console = 1,
			TestMongoDb = 2,
			MongoDb = 3,
			MySql = 4,
			MsSql = 5,
			JsonFile = 6,
			MySqlFile = 7
		}

		public abstract Types Type { get; internal set; }

		public abstract IEntityPipeline GetPipeline(Schema schema, Entity entityDefine);
	}

#if !NET_CORE
	public class MongoDbPipeline : Pipeline
	{
		public override Types Type { get; internal set; } = Types.MongoDb;

		public string ConnectString { get; set; }

		public override IEntityPipeline GetPipeline(Schema schema, Entity entityDefine)
		{
			return new EntityMongoDbPipeline(schema, ConnectString);
		}
	}

	public class TestMongoDbPipeline : Pipeline
	{
		public override Types Type { get; internal set; } = Types.TestMongoDb;

		public string ConnectString { get; set; }

		public string TaskId { get; set; }

		public override IEntityPipeline GetPipeline(Schema schema, Entity entityDefine)
		{
			return new EntityTestMongoDbPipeline(TaskId, schema, ConnectString);
		}
	}
#endif

	public class MysqlFilePipeline : Pipeline
	{
		public override Types Type { get; internal set; } = Types.MySqlFile;

		public override IEntityPipeline GetPipeline(Schema schema, Entity entityDefine)
		{
			return new EntityMySqlFilePipeline(schema, entityDefine);
		}
	}

	public class MysqlPipeline : Pipeline
	{
		public override Types Type { get; internal set; } = Types.MySql;

		public PipelineMode Mode { get; set; } = PipelineMode.Insert;

		public string ConnectString { get; set; }

		public override IEntityPipeline GetPipeline(Schema schema, Entity entityDefine)
		{
			return new EntityMySqlPipeline(schema, entityDefine, ConnectString, Mode);
		}
	}

	public class ConslePipeline : Pipeline
	{
		public override Types Type { get; internal set; } = Types.Console;

		public override IEntityPipeline GetPipeline(Schema schema, Entity entityDefine)
		{
			return new EntityConsolePipeline();
		}
	}
}

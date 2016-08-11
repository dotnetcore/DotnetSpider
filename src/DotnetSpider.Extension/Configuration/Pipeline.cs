using DotnetSpider.Extension.ORM;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extension.Model;

namespace DotnetSpider.Extension.Configuration
{
	public abstract class Pipeline
	{
		public abstract IEntityPipeline GetPipeline(Schema schema, EntityMetadata entityDefine);
	}

#if !NET_CORE
	public class MongoDbPipeline : Pipeline
	{
		public string ConnectString { get; set; }

		public override IEntityPipeline GetPipeline(Schema schema, EntityMetadata entityDefine)
		{
			return new EntityMongoDbPipeline(schema, ConnectString);
		}
	}

	//public class TestMongoDbPipeline : Pipeline
	//{
	//	public override Types Type { get; internal set; } = Types.TestMongoDb;

	//	public string ConnectString { get; set; }

	//	public string TaskId { get; set; }

	//	public override IEntityPipeline GetPipeline(Schema schema, EntityMetadata entityDefine)
	//	{
	//		return new EntityTestMongoDbPipeline(TaskId, schema, ConnectString);
	//	}
	//}
#endif

	public class MysqlFilePipeline : Pipeline
	{
		public override IEntityPipeline GetPipeline(Schema schema, EntityMetadata entityDefine)
		{
			return new EntityMySqlFilePipeline(schema, entityDefine);
		}
	}

	public class MysqlPipeline : Pipeline
	{
		public PipelineMode Mode { get; set; } = PipelineMode.Insert;

		public string ConnectString { get; set; }

		public override IEntityPipeline GetPipeline(Schema schema, EntityMetadata entityDefine)
		{
			return new EntityMySqlPipeline(schema, entityDefine, ConnectString, Mode);
		}
	}

	public class ConslePipeline : Pipeline
	{
		public override IEntityPipeline GetPipeline(Schema schema, EntityMetadata entityDefine)
		{
			return new EntityConsolePipeline();
		}
	}
}

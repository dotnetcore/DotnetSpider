using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Extension.ORM;
using Java2Dotnet.Spider.Extension.Pipeline;
using Newtonsoft.Json.Linq;

namespace Java2Dotnet.Spider.Extension.Configuration
{
	public abstract class Pipeline : IJobject
	{
		public enum Types
		{
			MongoDb,
			MySql,
			MsSql,
			JsonFile,
			MySqlFile
		}

		public abstract Types Type { get; internal set; }

		public abstract IEntityPipeline GetPipeline(Schema schema, JObject entityDefine);
	}

	public class MongoDbPipeline : Pipeline
	{
		public override Types Type { get; internal set; } = Types.MongoDb;

		public string Host { get; set; }
		public int Port { get; set; }
		public string Password { get; set; }

		public override IEntityPipeline GetPipeline(Schema schema, JObject entityDefine)
		{
			//return new EntityMongoDbPipeline(schema, Host, Port, Password);
			throw new SpiderExceptoin("UNSPORT");
		}
	}

	public class MysqlFilePipeline : Pipeline
	{
		public override Types Type { get; internal set; } = Types.MySqlFile;

		public override IEntityPipeline GetPipeline(Schema schema, JObject entityDefine)
		{
			return new EntityMySqlFilePipeline(schema, entityDefine);
		}
	}

	public class MysqlPipeline : Pipeline
	{
		public override Types Type { get; internal set; } = Types.MySql;

		public string ConnectString { get; set; }

		public override IEntityPipeline GetPipeline(Schema schema, JObject entityDefine)
		{
			return new EntityMySqlPipeline(schema, entityDefine, ConnectString);
		}
	}
}

#if !NET_CORE
using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.ORM;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace DotnetSpider.Extension.Pipeline
{
	public class MongoDbEntityPipeline : BaseEntityPipeline
	{
		public string ConnectString { get; set; }
		[JsonIgnore]
		public IUpdateConnectString UpdateConnectString { get; set; }
		protected Schema Schema { get; set; }
		private IMongoCollection<BsonDocument> _collection;

		public MongoDbEntityPipeline(string connectString)		{			ConnectString = connectString;		}

		public override void InitiEntity(EntityMetadata metadata)
		{
			if (metadata.Schema == null)
			{
				Logger.SaveLog(LogInfo.Create("Miss pipeline because: Schema is necessary", Logger.Name, Spider, LogLevel.Warn));
				IsEnabled = false;
				return;
			}

			Schema = BaseEntityDbPipeline.GenerateSchema(metadata.Schema);
			MongoClient client = new MongoClient(ConnectString);
			var db = client.GetDatabase(metadata.Schema.Database);

			_collection = db.GetCollection<BsonDocument>(metadata.Schema.TableName);
		}

		public override void Process(List<JObject> datas)
		{
			List<BsonDocument> reslut = new List<BsonDocument>();
			foreach (var data in datas)
			{
				BsonDocument item = BsonDocument.Parse(data.ToString());

				reslut.Add(item);
			}
			_collection.InsertMany(reslut);
		}

		public override BaseEntityPipeline Clone()
		{
			return new MongoDbEntityPipeline(ConnectString)
			{
				UpdateConnectString = UpdateConnectString
			};
		}

		public Schema GetSchema()
		{
			return Schema;
		}
	}
}
#endif
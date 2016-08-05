#if !NET_CORE

using System.Collections.Generic;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Extension.ORM;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace Java2Dotnet.Spider.Extension.Pipeline
{
	public class EntityMongoDbPipeline : EntityBasePipeline
	{
		private readonly IMongoCollection<BsonDocument> _collection;

		public EntityMongoDbPipeline(Schema schema, string connectString)
		{
			MongoClient client = new MongoClient(connectString);
			var db = client.GetDatabase(schema.Database);

			_collection = db.GetCollection<BsonDocument>(schema.TableName);
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
	}
}
#endif
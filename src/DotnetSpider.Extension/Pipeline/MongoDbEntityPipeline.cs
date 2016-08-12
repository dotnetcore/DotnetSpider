#if !NET_CORE

using System.Collections.Generic;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.ORM;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace DotnetSpider.Extension.Pipeline
{
	public class MongoDbEntityPipeline : BaseEntityPipeline
	{
		public string ConnectString { get; set; }
		private IMongoCollection<BsonDocument> _collection;

		public MongoDbEntityPipeline(string connectString)		{			ConnectString = connectString;		}

		public override void InitiEntity(Schema schema, EntityMetadata metadata)
		{
			MongoClient client = new MongoClient(ConnectString);
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
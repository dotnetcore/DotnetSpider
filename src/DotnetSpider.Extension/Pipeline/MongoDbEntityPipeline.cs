#if !NET_CORE
using System.Collections.Generic;
using DotnetSpider.Extension.Model;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace DotnetSpider.Extension.Pipeline
{
	public class MongoDbEntityPipeline : BaseEntityPipeline
	{
		public string ConnectString { get; set; }
		private IMongoCollection<BsonDocument> _collection;

		public override object Clone()
		{
			return new MongoDbEntityPipeline(ConnectString);
		}

		public MongoDbEntityPipeline(string connectString)		{			ConnectString = connectString;		}

		public override void InitiEntity(EntityMetadata metadata)
		{
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
			return  new MongoDbEntityPipeline(ConnectString);
		}
	}
}
#endif
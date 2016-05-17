#if !NET_CORE

using System.Collections.Generic;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Extension.ORM;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace Java2Dotnet.Spider.Extension.Pipeline
{
	public class EntityTestMongoDbPipeline : IEntityPipeline
	{
		private readonly IMongoCollection<BsonDocument> _collection;
		private readonly string _id;

		public EntityTestMongoDbPipeline(string id, Schema schema, string connectString)
		{
			MongoClient client = new MongoClient(connectString);
			var db = client.GetDatabase("test_data");

			_collection = db.GetCollection<BsonDocument>("testData");
			_id = id;
		}

		public void Initialize()
		{
		}

		public void Process(List<JObject> datas, ISpider spider)
		{
			List<BsonDocument> reslut = new List<BsonDocument>();
			foreach (var data in datas)
			{
				JObject obj = new JObject();

				obj.Add("taskId", _id);
				obj.Add("data", data);
				BsonDocument item = BsonDocument.Parse(obj.ToString());

				reslut.Add(item);
			}
			_collection.InsertMany(reslut);
		}

		public void Dispose()
		{
		}
	}
}
#endif
#if !NET_CORE

using System.Collections.Generic;
using DotnetSpider.Extension.ORM;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System;

namespace DotnetSpider.Extension.Pipeline
{
	public class EntityTestMongoDbPipeline : EntityBasePipeline
	{
		private readonly IMongoCollection<BsonDocument> _collection;
		private readonly string _id;

		public EntityTestMongoDbPipeline(string id, Schema schema, string connectString)
		{
			MongoClient client = new MongoClient(connectString);
			var db = client.GetDatabase("test_data");

			_collection = db.GetCollection<BsonDocument>("TestData");
			_id = id;
		}

		public override void Process(List<JObject> datas)
		{
			List<BsonDocument> reslut = new List<BsonDocument>();
			var time = DateTime.Now;
			foreach (var data in datas)
			{
				reslut.Add(new BsonDocument
				{
					{"TaskId",_id},
					{"Timestamp",time},
					{"Data", data.ToString()}
				});
			}
			_collection.InsertMany(reslut);
		}
	}
}
#endif
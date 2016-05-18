#if !NET_CORE

using System.Collections.Generic;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Extension.ORM;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System;

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

			_collection = db.GetCollection<BsonDocument>("data");
			_id = id;
		}

		public void Initialize()
		{
		}

		public void Process(List<JObject> datas, ISpider spider)
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

		public void Dispose()
		{
		}
	}
}
#endif
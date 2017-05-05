﻿using System.Collections.Generic;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.ORM;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DotnetSpider.Core.Infrastructure;
using System.Collections.Concurrent;

namespace DotnetSpider.Extension.Pipeline
{
	public class MongoDbEntityPipeline : BaseEntityPipeline
	{
		public string ConnectString { get; set; }
		[JsonIgnore]
		public IUpdateConnectString UpdateConnectString { get; set; }

		protected ConcurrentDictionary<string, IMongoCollection<BsonDocument>> Collections = new ConcurrentDictionary<string, IMongoCollection<BsonDocument>>();

		public MongoDbEntityPipeline(string connectString)
		{
			ConnectString = connectString;
		}

		public override void AddEntity(EntityMetadata metadata)
		{
			base.AddEntity(metadata);

			if (metadata.Table == null)
			{
				return;
			}

			MongoClient client = new MongoClient(ConnectString);
			var db = client.GetDatabase(metadata.Table.Database);

			Collections.TryAdd(metadata.Table.Name, db.GetCollection<BsonDocument>(metadata.Table.Name));
		}

		public override void Process(string entityName, List<JObject> datas)
		{
			IMongoCollection<BsonDocument> collection;
			if (Collections.TryGetValue(entityName, out collection))
			{
				List<BsonDocument> reslut = new List<BsonDocument>();
				foreach (var data in datas)
				{
					BsonDocument item = BsonDocument.Parse(data.ToString());

					reslut.Add(item);
				}
				collection.InsertMany(reslut);
			}
		}
	}
}
using System.Collections.Generic;
using DotnetSpider.Extension.Model;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Concurrent;
using System;
using DotnetSpider.Core.Infrastructure;
using NLog;
using DotnetSpider.Extension.Infrastructure;

namespace DotnetSpider.Extension.Pipeline
{
	public class MongoDBEntityPipeline : BaseEntityPipeline
	{
		protected ConcurrentDictionary<string, IMongoCollection<BsonDocument>> Collections = new ConcurrentDictionary<string, IMongoCollection<BsonDocument>>();

		public string ConnectString { get; set; }

		public IUpdateConnectString UpdateConnectString { get; set; }

		public MongoDBEntityPipeline(string connectString)
		{
			ConnectString = connectString;
		}

		public override void AddEntity(EntityDefine metadata)
		{
			base.AddEntity(metadata);

			if (metadata.Table == null)
			{
				Logger.MyLog(Spider?.Identity, $"Schema is necessary, skip {GetType().Name} for {metadata.Name}.", LogLevel.Warn);
				return;
			}

			MongoClient client = new MongoClient(ConnectString);
			var db = client.GetDatabase(metadata.Table.Database);

			Collections.TryAdd(metadata.Table.Name, db.GetCollection<BsonDocument>(metadata.Table.Name));
		}

		public override void Process(string entityName, List<DataObject> datas)
		{
			IMongoCollection<BsonDocument> collection;
			if (Collections.TryGetValue(entityName, out collection))
			{
				List<BsonDocument> reslut = new List<BsonDocument>();
				foreach (var data in datas)
				{
					BsonDocument item = new BsonDocument(data);

					reslut.Add(item);
				}
				reslut.Add(BsonDocument.Create(DateTime.Now));
				collection.InsertMany(reslut);
			}
		}
	}
}
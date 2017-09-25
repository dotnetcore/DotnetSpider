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
	public class MongoDbEntityPipeline : BaseEntityPipeline
	{
		protected ConcurrentDictionary<string, IMongoCollection<BsonDocument>> Collections = new ConcurrentDictionary<string, IMongoCollection<BsonDocument>>();

		public string ConnectString { get; set; }

		public IUpdateConnectString UpdateConnectString { get; set; }

		public MongoDbEntityPipeline(string connectString)
		{
			ConnectString = connectString;
		}

		public override void AddEntity(EntityDefine metadata)
		{
			base.AddEntity(metadata);

			if (metadata.TableInfo == null)
			{
				Logger.MyLog(Spider?.Identity, $"Schema is necessary, skip {GetType().Name} for {metadata.Name}.", LogLevel.Warn);
				return;
			}

			MongoClient client = new MongoClient(ConnectString);
			var db = client.GetDatabase(metadata.TableInfo.Database);

			Collections.TryAdd(metadata.Name, db.GetCollection<BsonDocument>(metadata.TableInfo.CalculateTableName()));
		}

		public override int Process(string entityName, List<DataObject> datas)
		{
			if (Collections.TryGetValue(entityName, out var collection))
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
			return datas.Count;
		}
	}
}
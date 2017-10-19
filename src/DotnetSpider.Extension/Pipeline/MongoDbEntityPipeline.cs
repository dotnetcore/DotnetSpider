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
		private readonly ConcurrentDictionary<string, IMongoCollection<BsonDocument>> _collections = new ConcurrentDictionary<string, IMongoCollection<BsonDocument>>();

		private string ConnectString { get; }

		public MongoDbEntityPipeline(string connectString)
		{
			ConnectString = connectString;
		}

		public override void AddEntity(IEntityDefine metadata)
		{
			if (metadata.TableInfo == null)
			{
				Logger.MyLog(Spider?.Identity, $"Schema is necessary, skip {GetType().Name} for {metadata.Name}.", LogLevel.Warn);
				return;
			}

			MongoClient client = new MongoClient(ConnectString);
			var db = client.GetDatabase(metadata.TableInfo.Database);

			_collections.TryAdd(metadata.Name, db.GetCollection<BsonDocument>(metadata.TableInfo.CalculateTableName()));
		}

		public override int Process(string entityName, List<dynamic> datas)
		{
			if (_collections.TryGetValue(entityName, out var collection))
			{
				List<BsonDocument> reslut = new List<BsonDocument>();
				foreach (var data in datas)
				{
					BsonDocument item = BsonDocument.Create(data);
					reslut.Add(item);
				}
				reslut.Add(BsonDocument.Create(DateTime.Now));
				collection.InsertMany(reslut);
			}
			return datas.Count;
		}
	}
}
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DotnetSpider.Common;
using DotnetSpider.DataFlow.Storage.Model;
using MongoDB.Bson;
using MongoDB.Driver;

[assembly: InternalsVisibleTo("DotnetSpider.Tests")]

// ReSharper disable once CheckNamespace
namespace DotnetSpider.DataFlow.Storage.Mongo
{
	/// <summary>
	/// MongoDB 保存解析(实体)结果 TODO: 是否要考虑存储模式：插入，新的插入旧的更新，更新 ETC
	/// </summary>
	public class MongoEntityStorage : EntityStorageBase
	{
		private readonly IMongoClient _client;

		private readonly ConcurrentDictionary<string, IMongoDatabase> _cache =
			new ConcurrentDictionary<string, IMongoDatabase>();

		/// <summary>
		/// 根据配置返回存储器
		/// </summary>
		/// <param name="options">配置</param>
		/// <returns></returns>
		public static MongoEntityStorage CreateFromOptions(SpiderOptions options)
		{
			return new MongoEntityStorage(options.StorageConnectionString);
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="connectionString">连接字符串</param>
		public MongoEntityStorage(string connectionString)
		{
			ConnectionString = connectionString;
			_client = new MongoClient(connectionString);
		}

		internal MongoEntityStorage(IMongoClient mongoClient)
		{
			_client = mongoClient;
		}

		/// <summary>
		/// 连接字符串
		/// </summary>
		public string ConnectionString { get; }

		protected override async Task<DataFlowResult> Store(DataFlowContext context)
		{
			var items = context.GetData();
			foreach (var item in items)
			{
				var tableMetadata = (TableMetadata) context[item.Key];

				if (!_cache.ContainsKey(tableMetadata.Schema.Database))
				{
					_cache.TryAdd(tableMetadata.Schema.Database, _client.GetDatabase(tableMetadata.Schema.Database));
				}

				var db = _cache[tableMetadata.Schema.Database];
				var collection = db.GetCollection<BsonDocument>(tableMetadata.Schema.Table);

				var bsonDocs = new List<BsonDocument>();
				foreach (var data in item.Value)
				{
					bsonDocs.Add(BsonDocument.Create(data));
				}

				await collection.InsertManyAsync(bsonDocs);
			}

			return DataFlowResult.Success;
		}
	}
}

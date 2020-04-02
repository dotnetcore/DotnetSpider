using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DotnetSpider.DataFlow;
using DotnetSpider.DataFlow.Storage;
using MongoDB.Bson;
using MongoDB.Driver;

[assembly: InternalsVisibleTo("DotnetSpider.Tests")]

namespace DotnetSpider.Mongo
{
    /// <summary>
    /// MongoDB 保存解析(实体)结果 TODO: 是否要考虑存储模式：插入，新的插入旧的更新，更新 ETC
    /// </summary>
    public class MongoEntityStorage : EntityStorageBase
    {
        private readonly IMongoClient _client;

        private readonly ConcurrentDictionary<Type, TableMetadata> _tableMetadatas =
            new ConcurrentDictionary<Type, TableMetadata>();

        private readonly ConcurrentDictionary<string, IMongoDatabase> _cache =
            new ConcurrentDictionary<string, IMongoDatabase>();

        public static IDataFlow CreateFromOptions(SpiderOptions options)
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

        protected override async Task StoreAsync(DataContext context, Dictionary<Type, List<dynamic>> dict)
        {
            foreach (var kv in dict)
            {
                var list = (IList) kv.Value;
                var tableMetadata = _tableMetadatas.GetOrAdd(kv.Key,
                    type => ((IEntity) list[0]).GetTableMetadata());

                if (string.IsNullOrWhiteSpace(tableMetadata.Schema.Database))
                {
                    throw new ArgumentException("Database of schema should not be empty or null");
                }

                if (!_cache.ContainsKey(tableMetadata.Schema.Database))
                {
                    _cache.TryAdd(tableMetadata.Schema.Database, _client.GetDatabase(tableMetadata.Schema.Database));
                }

                var db = _cache[tableMetadata.Schema.Database];
                var collection = db.GetCollection<BsonDocument>(tableMetadata.Schema.Table);

                var bsonDocs = new List<BsonDocument>();
                foreach (var data in list)
                {
                    bsonDocs.Add(data.ToBsonDocument());
                }

                await collection.InsertManyAsync(bsonDocs);
            }
        }
    }
}
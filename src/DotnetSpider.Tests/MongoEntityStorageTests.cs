using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotnetSpider.DataFlow;
using DotnetSpider.DataFlow.Storage;
using DotnetSpider.Mongo;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace DotnetSpider.Tests
{
    public class MongoEntityStorageTests
    {
        [Schema("test", "CreateTableEntity1")]
        public class CreateTableEntity1 : EntityBase<CreateTableEntity1>
        {
            public string Str1 { get; set; } = "xxx";

            public string Str2 { get; set; } = "yyy";

            public int Required1 { get; set; } = 655;

            public decimal Decimal1 { get; set; }

            public long Long1 { get; set; } = 600;

            public double Double1 { get; set; } = 400;

            public DateTimeOffset DateTime1 { get; set; } = DateTimeOffset.Now;

            public DateTimeOffset DateTimeOffset1 { get; set; } = DateTimeOffset.Now;

            public float Float1 { get; set; } = 200.0F;
        }

        /// <summary>
        /// 测试芒果数据库存储数据成功
        /// 1. 数据库名是否正确
        /// 2. Collection 是否正确
        /// 3. 数据存储是否正确
        /// </summary>
        [Fact]
        public async Task Store_Should_Success()
        {
            var mongoCollection = new Mock<IMongoCollection<BsonDocument>>();

            var mongoDatabase = new Mock<IMongoDatabase>();
            mongoDatabase.Setup(d =>
                    d.GetCollection<BsonDocument>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
                .Returns(mongoCollection.Object);

            var mongoClient = new Mock<IMongoClient>();
            mongoClient.Setup(d => d.GetDatabase(It.IsAny<string>(), It.IsAny<MongoDatabaseSettings>()))
                .Returns(mongoDatabase.Object);

            var mongoEntityStorage = new MongoEntityStorage(mongoClient.Object);

            var dfc = new DataFlowContext(null, null, null, null);
            var typeName = typeof(CreateTableEntity1);
            var entity = new CreateTableEntity1();

            var items = new List<CreateTableEntity1>
            {
                entity
            };
            dfc.AddData(typeName, items);
            await mongoEntityStorage.HandleAsync(dfc);
        }


        [Fact]
        public async Task Store_Empty_Should_Success()
        {
            var mongoClient = new Mock<IMongoClient>();
            var mongoEntityStorage = new MongoEntityStorage(mongoClient.Object);

            var dfc = new DataFlowContext(null, null, null, null);
            mongoEntityStorage.SetLogger(NullLogger.Instance);
            await mongoEntityStorage.HandleAsync(dfc);
        }
    }
}

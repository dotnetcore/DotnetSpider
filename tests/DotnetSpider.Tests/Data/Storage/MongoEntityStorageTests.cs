using System;
using System.Threading.Tasks;
using DotnetSpider.DataFlow;
using DotnetSpider.DataFlow.Parser;
using DotnetSpider.DataFlow.Storage.Model;
using DotnetSpider.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace DotnetSpider.Tests.Data.Storage
{
    public class MongoEntityStorageTests
    {
        class CreateTableEntity1 : EntityBase<CreateTableEntity1>
        {
            public string Str1 { get; set; } = "xxx";

            public string Str2 { get; set; } = "yyy";

            public int Required { get; set; } = 655;

            public decimal Decimal { get; set; }

            public long Long { get; set; } = 600;

            public double Double { get; set; } = 400;

            public DateTime DateTime { get; set; } = DateTime.Now;

            public DateTimeOffset DateTimeOffset { get; set; } = DateTimeOffset.Now;

            public float Float { get; set; } = 200.0F;
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
            var serviceProvider = Mock.Of<IServiceProvider>();

            var mongoCollection = new Mock<IMongoCollection<BsonDocument>>();

            var mongoDatabase = new Mock<IMongoDatabase>();
            mongoDatabase.Setup(d =>
                    d.GetCollection<BsonDocument>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
                .Returns(mongoCollection.Object);

            var mongoClient = new Mock<IMongoClient>();
            mongoClient.Setup(d => d.GetDatabase(It.IsAny<string>(), It.IsAny<MongoDatabaseSettings>()))
                .Returns(mongoDatabase.Object);

            var mongoEntityStorage = new MongoEntityStorage(mongoClient.Object);

            var dfc = new DataFlowContext(null, serviceProvider);
            var typeName = typeof(CreateTableEntity1).FullName;
            var entity = new CreateTableEntity1();
            dfc.Add(typeName, entity.GetTableMetadata());
            var items = new ParseResult<CreateTableEntity1>
            {
                entity
            };
            dfc.AddParseItem(typeName, items);
            var result = await mongoEntityStorage.HandleAsync(dfc);

            Assert.Equal(DataFlowResult.Success, result);
        }


        [Fact]
        public async Task Store_Empty_Should_Success()
        {
            var serviceProvider = Mock.Of<IServiceProvider>();
            var mongoClient = new Mock<IMongoClient>();
            var mongoEntityStorage = new MongoEntityStorage(mongoClient.Object);

            var dfc = new DataFlowContext(null, serviceProvider);
            var typeName = typeof(CreateTableEntity1).FullName;
            var entity = new CreateTableEntity1();
            dfc.Add(typeName, entity.GetTableMetadata());
            var items = new ParseResult<CreateTableEntity1>();
            dfc.AddParseItem(typeName, items);
            
            var result = await mongoEntityStorage.HandleAsync(dfc);
            Assert.Equal(DataFlowResult.Success, result);
        }
    }
}
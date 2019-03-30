using Xunit;

namespace DotnetSpider.Tests.Data.Storage
{
    public class PostgreSqlEntityStorageTests
    {
        /// <summary>
        /// 测试能正确创建 MySql 数据库
        /// </summary>
        [Fact]
        public void CreateDatabase()
        {
            // TODO
        }

        /// <summary>
        /// 测试能正确创建 MySql 表
        /// 1. 如果实体的 Schema 没有配置表名，则使用类名
        /// 2. 如果实体的 Schema 配置了表名，则使用配置的表名
        /// 3. 是否有正确添加表的后缀
        /// </summary>
        [Fact]
        public void CreateTable()
        {
            // TODO
        }

        /// <summary>
        /// 测试能正确插入数据
        /// </summary>
        [Fact]
        public void Insert()
        {
            // TODO
        }

        /// <summary>
        /// 测试能正确插入数据，如果遇到重复数据则忽略插入
        /// </summary>
        [Fact]
        public void InsertIgnoreDuplicate()
        {
            // TODO
        }

        /// <summary>
        /// 测试 如果遇到重复数据则更新，主键不重复则插入
        /// 1. 此模式必须配置有主键，无主键效率太低
        /// 2. 更新则是全量更新
        /// </summary>
        [Fact]
        public void InsertAndUpdate()
        {
            // TODO
        }

        /// <summary>
        /// 测试能否正确更新数据
        /// </summary>
        [Fact]
        public void Update()
        {
            // TODO
        }
        
        /// <summary>
        /// 测试事务能否正常开启
        /// </summary>
        [Fact]
        public void UseTransaction()
        {
        }
        
        /// <summary>
        /// 测试数据库名，表名，列名是的大小写是否正确
        /// </summary>
        [Fact]
        public void IgnoreCase()
        {
        }
    }
}
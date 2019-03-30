using Xunit;

namespace DotnetSpider.Tests.Data.Storage.Model
{
    public class TableMetadataTests
    {
        /// <summary>
        /// 测试实体模型的 TypeName 是否解析到 TableMetadata 对象中
        /// </summary>
        [Fact(DisplayName = "TableMetadataTypeName")]
        public void TableMetadataTypeName()
        {
            // TODO
        }

        /// <summary>
        /// 测试实体模型的 Schema 是否解析到 TableMetadata 对象中
        /// 1. 无
        /// 2. 单个
        /// 3. 多个
        /// </summary>
        [Fact(DisplayName = "TableMetadataSchema")]
        public void TableMetadataSchema()
        {
            // TODO
        }

        /// <summary>
        /// 测试实体模型的 Primary 是否解析到 TableMetadata 对象中： EntityBase.HasKey 配置
        /// 1. 没有配置，但是有名为 Id 的属性
        /// 2. 配置单列为主键
        /// 3. 配置多列为主键
        /// 4. HasKey 配置相同的数据
        /// </summary>
        [Fact(DisplayName = "TableMetadataPrimary")]
        public void TableMetadataPrimary()
        {
            // TODO
        }

        /// <summary>
        /// 测试实体模型的 Indexes 是否解析到 TableMetadata 对象中： EntityBase.HasIndex 配置
        /// 1. 没有配置
        /// 2. 配置 1 个单列为索引
        /// 3. 配置 1 个多列为索引
        /// 4. 配置多个索引
        /// </summary>
        [Fact(DisplayName = "TableMetadataIndexes")]
        public void TableMetadataIndexes()
        {
            // TODO
        }

        /// <summary>
        /// 测试实体模型的 Columns 是否解析到 TableMetadata 对象中
        /// Columns 为所有 Publish 有 Set, Get 的属性
        /// 1. Set, Get 的属性是否有正确解析
        /// 2. Set 属性是否没有在字典里
        /// 3. Get 属性是否没有在字典里
        /// 4. 私有属性、保护属性、internal 属性是否不在字典里
        /// 5. public field 是否没有在字典里
        /// 6. 
        /// </summary>
        [Fact(DisplayName = "TableMetadataColumns")]
        public void TableMetadataColumns()
        {
            // TODO
        }

        /// <summary>
        /// 测试 StringLength attribute 是否有解析正确到 Columns 中
        /// </summary>
        [Fact(DisplayName = "TableMetadataColumnLength")]
        public void TableMetadataColumnLength()
        {
            // TODO
        }

        /// <summary>
        /// 检测 IsAutoIncrementPrimary  返回值是否正确
        /// 只有在主键只有一列，并且例的数据类型为 int, long 时才返回自增
        /// </summary>
        [Fact(DisplayName = "TableMetadataIsAutoIncrementPrimary")]
        public void TableMetadataIsAutoIncrementPrimary()
        {
        }
        
        /// <summary>
        /// 测试 EntityBase.ConfigureUpdateColumns 是否执行正确
        /// </summary>
        [Fact(DisplayName = "TableMetadataUpdate")]
        public void TableMetadataUpdate()
        {
            // TODO
        }
    }
}
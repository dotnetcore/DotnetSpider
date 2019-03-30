using DotnetSpider.Data.Parser;
using DotnetSpider.Data.Parser.Attribute;
using DotnetSpider.Data.Storage.Model;
using DotnetSpider.Selector;
using Xunit;

namespace DotnetSpider.Tests.Data.Parser
{
    public class ModelTests
    {
        /// <summary>
        /// 测试实体模型的 TypeName 是否解析到 Model 对象中
        /// </summary>
        [Fact(DisplayName = "ModelTypeName")]
        public void ModelTypeName()
        {
            var model = new Model<ModelType>();
            Assert.Equal(model.TypeName, typeof(ModelType).FullName);
        }

        private class ModelType : EntityBase<ModelType>
        {
        }

        /// <summary>
        /// 测试实体模型上的 EntitySelector 是否解析到 Model 对象中
        /// 1. Type
        /// 2. Expression
        /// 3. Arguments
        /// 4. Take
        /// 5. TakeFromHead
        /// </summary>
        [Fact(DisplayName = "EntitySelector")]
        public void EntitySelector()
        {
            var model = new Model<Entity>();

            Assert.Equal(SelectorType.Css, model.Selector.Type);
            Assert.Equal("exp", model.Selector.Expression);
            Assert.Equal(10, model.Take);
            Assert.False(model.TakeFromHead);
        }

        [EntitySelector(Expression = "exp", Type = SelectorType.Css, Take = 10, TakeFromHead = false,
            Arguments = "args")]
        private class Entity : EntityBase<Entity>
        {
        }


        /// <summary>
        /// 测试实体模型上的 ValueSelector 有没有正确解析到 Model 对象中
        /// 1. 无 ValueSelector
        /// 2. 单个 ValueSelector
        /// 3. 多个 ValueSelector 无重复
        /// 4. 多个 ValueSelector 并有重复
        /// </summary>
        [Fact(DisplayName = "ShareValueSelectors")]
        public void ShareValueSelectors()
        {
            // TODO
        }

        /// <summary>
        /// 测试实体模型上的 FollowSelectors 有没有正确解析到 Model 对象中
        /// 1. 无 FollowSelector
        /// 2. 单个 FollowSelector
        /// 3. 多个 FollowSelector 无重复
        /// 4. 多个 FollowSelector 并有重复
        /// </summary>
        [Fact(DisplayName = "FollowSelectors")]
        public void FollowSelectors()
        {
            // TODO
        }
    }
}
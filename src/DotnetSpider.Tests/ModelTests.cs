using DotnetSpider.DataFlow.Parser;
using DotnetSpider.DataFlow.Storage;
using DotnetSpider.Selector;
using Xunit;

namespace DotnetSpider.Tests
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
            Assert.False(model.TakeByDescending);
        }

        [EntitySelector(Expression = "exp", Type = SelectorType.Css, Take = 10, TakeByDescending = false,
            Arguments = "args")]
        private class Entity : EntityBase<Entity>
        {
        }

    }
}

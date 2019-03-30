using System.ComponentModel.DataAnnotations;
using DotnetSpider.Data.Parser.Attribute;
using DotnetSpider.Data.Storage.Model;
using DotnetSpider.Selector;

namespace DotnetSpider.Data
{
    [Schema("db1", "tb1", TablePostfix = TablePostfix.Today)]
    [ValueSelector(Expression = "ValueSelector", Type = SelectorType.XPath)]
    [EntitySelector(Expression = "EntitySelector", Type = SelectorType.XPath)]
    public class TestModel : EntityBase<TestModel>
    {
        public int Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        public int Age { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [StringLength(100)]
        [ValueSelector(Expression = "ValueSelector", Type = SelectorType.XPath)]
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [StringLength(200)]
        public string Class { get; set; }

        protected override void Configure()
        {
            HasKey(x => x.Id).HasIndex(x => x.Name);
            HasIndex(x => new {x.Name, x.Class}, true);
        }
    }
}
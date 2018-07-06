using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Model.Attribute;

namespace DotnetSpider.Extension.Model
{
	public class BaseEntity
	{
		[Field(DataType = DataType.Long, IsPrimary = true, Expression = "Id", Type = SelectorType.Enviroment)]
		// ReSharper disable once UnusedAutoPropertyAccessor.Global
		public long Id { get; set; }
	}
}

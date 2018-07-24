using DotnetSpider.Extraction.Model.Attribute;

namespace DotnetSpider.Extraction.Model
{
	public class BaseEntity
	{
		[FieldSelector(DataType = DataType.Long, IsPrimary = true, Expression = "Id", Type = SelectorType.Enviroment)]
		// ReSharper disable once UnusedAutoPropertyAccessor.Global
		public long Id { get; set; }
	}
}

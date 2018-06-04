using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Model.Attribute;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetSpider.Extension.Model
{
	public class BaseEntity
	{
		[Field(DataType = DataType.Long, IsPrimary = true, Expression = "Id", Type = SelectorType.Enviroment)]
		public long Id { get; set; }
	}
}

using Cassandra;
using DotnetSpider.Extension.Model.Attribute;
using System;

namespace DotnetSpider.Extension.Model
{
	public interface ISpiderEntity
	{
	}

	public abstract class SpiderEntity : ISpiderEntity
	{
		[PropertyDefine(Expression = "__Id", Type = Core.Selector.SelectorType.Enviroment)]
		public long __Id { get; set; }

		[PropertyDefine(Expression = "now", Type = Core.Selector.SelectorType.Enviroment)]
		public DateTime CDate { get; set; }
	}

	public abstract class CassandraSpiderEntity : ISpiderEntity
	{
		[PropertyDefine(Expression = "Id", Type = Core.Selector.SelectorType.Enviroment)]
		public TimeUuid Id { get; set; }

		[PropertyDefine(Expression = "now", Type = Core.Selector.SelectorType.Enviroment)]
		public DateTime CDate { get; set; }
	}
}

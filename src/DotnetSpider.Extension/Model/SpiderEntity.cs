using Cassandra;
using DotnetSpider.Extension.Model.Attribute;
using System;

namespace DotnetSpider.Extension.Model
{
	public interface ISpiderEntity
	{
	}

	public interface ISpiderEntity<T> : ISpiderEntity
	{
		T Id { get; set; }
	}

	public abstract class SpiderEntity : ISpiderEntity<long>
	{
		[PropertyDefine(Expression = "null", Type = Core.Selector.SelectorType.Enviroment)]
		public long Id { get; set; }

		[PropertyDefine(Expression = "now", Type = Core.Selector.SelectorType.Enviroment)]
		public DateTime CDate { get; set; }
	}

	public abstract class CassandraSpiderEntity : ISpiderEntity<TimeUuid>
	{
		[PropertyDefine(Expression = "timeuuid", Type = Core.Selector.SelectorType.Enviroment)]
		public TimeUuid Id { get; set; }

		[PropertyDefine(Expression = "now", Type = Core.Selector.SelectorType.Enviroment)]
		public DateTime CDate { get; set; }
	}
}

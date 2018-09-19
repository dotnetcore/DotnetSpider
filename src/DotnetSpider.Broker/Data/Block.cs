using System;

namespace DotnetSpider.Broker.Data
{
	public enum BlockState
	{
		Ready,
		Using,
		Complete,
		Failed
	}

	public class Block : AuditedEntity<Guid>
	{
		public virtual Guid Identity { get; set; }

		public virtual BlockState State { get; set; }
	}
}

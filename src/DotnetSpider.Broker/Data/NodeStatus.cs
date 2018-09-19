using System;
using System.ComponentModel.DataAnnotations;

namespace DotnetSpider.Broker.Data
{
	public class NodeStatus : HasCreationTimeEntity
	{
		[Required]
		public virtual Guid NodeId { get; set; }

		[Required]
		public virtual int ProcessCount { get; set; }

		[Required]
		public virtual int Cpu { get; set; }

		[Required]
		public virtual int FreeMemory { get; set; }
	}
}

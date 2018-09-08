using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

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

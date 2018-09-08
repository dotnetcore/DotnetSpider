using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetSpider.Broker.Data
{
	public class Worker : AuditedEntity<int>
	{
		[Required]
		[StringLength(200)]
		public virtual string FullClassName { get; set; }

		[Required]
		[StringLength(50)]
		public virtual string ConnectionId { get; set; }
	}
}

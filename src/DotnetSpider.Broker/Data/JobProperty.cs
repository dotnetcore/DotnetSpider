using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetSpider.Broker.Data
{
	public class JobProperty : Entity
	{
		[Required]
		public virtual Guid JobId { get; set; }

		[Required]
		[StringLength(50)]
		public virtual string Key { get; set; }

		[Required]
		[StringLength(500)]
		public virtual string Value { get; set; }
	}
}

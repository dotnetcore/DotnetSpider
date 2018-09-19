using System;
using System.ComponentModel.DataAnnotations;

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

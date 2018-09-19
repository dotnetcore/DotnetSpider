using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DotnetSpider.Broker.Data
{
	public enum JobType
	{
		Application,
		Block
	}

	public class Job : FullAuditedEntity<Guid>
	{
		[Required]
		[StringLength(50)]
		public virtual string Name { get; set; }

		[Required]
		public virtual JobType JobType { get; set; }

		[Required]
		[StringLength(50)]
		public virtual string Cron { get; set; }

		[Required]
		[StringLength(500)]
		public virtual string Description { get; set; }

		public virtual bool IsEnabled { get; set; }

		public virtual IList<JobProperty> Properties { get; set; }
	}
}

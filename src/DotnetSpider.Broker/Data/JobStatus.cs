using DotnetSpider.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetSpider.Broker.Data
{
	public class JobStatus : Entity<Guid>, IHasModificationTime
	{
		public virtual Guid Identity { get; set; }
		public virtual Guid NodeId { get; set; }
		public virtual Status Status { get; set; }
		public virtual string Detail { get; set; }
		public virtual DateTime? LastModificationTime { get; set; }
	}
}

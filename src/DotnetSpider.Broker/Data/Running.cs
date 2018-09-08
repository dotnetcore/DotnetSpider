using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetSpider.Broker.Data
{
	public class Running : Entity<Guid>, IHasCreationTime
	{
		public virtual Guid Identity { get; set; }

		public virtual int Consuming { get; set; }

		public virtual int Priority { get; set; }

		public virtual DateTime CreationTime { get; set; }
	}
}

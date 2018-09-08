using System;
using System.ComponentModel.DataAnnotations;

namespace DotnetSpider.Broker.Data
{
	public class Node : Entity<Guid>, IHasCreationTime
	{
		/// <summary>
		/// Console or Web
		/// </summary>
		[Required]
		[StringLength(10)]
		public virtual string NodeType { get; set; }

		[Required]
		[StringLength(50)]
		public virtual string ConnectionId { get; set; }

		[Required]
		[StringLength(50)]
		public virtual string IpAddress { get; set; }

		[Required]
		public virtual int ProcessorCount { get; set; }

		[Required]
		[StringLength(50)]
		public virtual string Group { get; set; }

		[Required]
		[StringLength(50)]
		public virtual string OperatingSystem { get; set; }

		[Required]
		public virtual int Memory { get; set; }

		public virtual bool IsEnabled { get; set; }

		/// <summary>
		/// An entity can implement this interface if <see cref="LastModificationTime"/> of this entity must be stored.
		/// <see cref="LastModificationTime"/> is automatically set when updating <see cref="Entity"/>.
		/// </summary>
		public virtual DateTime CreationTime { get; set; }
	}
}

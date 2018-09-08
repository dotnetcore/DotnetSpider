using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetSpider.Broker.Dtos
{
	public class AddNodeStatusDto
	{
		[Required]
		public virtual int ProcessCount { get; set; }

		[Required]
		public virtual int Cpu { get; set; }

		[Required]
		public virtual int FreeMemory { get; set; }
	}
}

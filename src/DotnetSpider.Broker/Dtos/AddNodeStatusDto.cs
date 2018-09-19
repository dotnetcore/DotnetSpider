using System.ComponentModel.DataAnnotations;

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

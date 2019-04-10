using System;
using System.ComponentModel.DataAnnotations;

namespace DotnetSpider.Portal.Entity
{
	public class DockerImageRepository
	{
		public int Id { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[StringLength(255)]
		[Required]
		public string Name { get; set; }

		/// <summary>
		/// registry.cn-shanghai.aliyuncs.com
		/// </summary>
		[StringLength(255)]
		[Required]
		public string Registry { get; set; }
		
		/// <summary>
		/// 
		/// </summary>
		[StringLength(255)]
		[Required]
		public string Repository { get; set; }

		/// <summary>
		/// Creation time of this entity.
		/// </summary>
		[Required]
		public DateTime CreationTime { get; set; }			
	}
}
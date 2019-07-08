using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotnetSpider.Portal.Entity
{
	[Table("docker_repository")]
	public class DockerRepository
	{
		/// <summary>
		/// 主键
		/// </summary>
		[Column("id")]
		public int Id { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[StringLength(255)]
		[Required]
		[Column("name")]
		public string Name { get; set; }

		/// <summary>
		/// http://registry.cn-shanghai.aliyuncs.com/
		/// </summary>
		[StringLength(255)]
		[Required]
		[Column("registry")]
		public string Registry { get; set; }

		/// <summary>
		/// zlzforever/ids4admin
		/// </summary>
		[StringLength(255)]
		[Required]
		[Column("repository")]
		public string Repository { get; set; }

		[Column("user_name")]
		[StringLength(255)]
		public string UserName { get; set; }

		[Column("password")]
		[StringLength(255)]
		public string Password { get; set; }

		/// <summary>
		/// Creation time of this entity.
		/// </summary>
		[Required]
		[Column("creation_time")]
		public DateTime CreationTime { get; set; }
	}
}
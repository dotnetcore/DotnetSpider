using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotnetSpider.Portal.Data
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
		/// http:// or https://
		/// </summary>
		[Column("schema")]
		[StringLength(10)]
		public string Schema { get; set; }

		/// <summary>
		/// registry.cn-shanghai.aliyuncs.com/ 允许为空，表示本地镜像
		/// </summary>
		[StringLength(255)]
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
		public DateTimeOffset CreationTime { get; set; }
	}
}

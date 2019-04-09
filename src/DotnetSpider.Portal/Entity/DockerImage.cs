using System;
using System.ComponentModel.DataAnnotations;

namespace DotnetSpider.Portal.Entity
{
	public class DockerImage
	{
		public int Id { get; set; }

		public int DockerImageRepositoryId { get; set; }

		/// <summary>
		/// registry.cn-shanghai.aliyuncs.com/zlzforever/helloworld:20190409.22
		/// </summary>
		[StringLength(255)]
		public string Repository { get; set; }

		/// <summary>
		/// Creation time of this entity.
		/// </summary>
		public DateTime CreationTime { get; set; }
	}
}
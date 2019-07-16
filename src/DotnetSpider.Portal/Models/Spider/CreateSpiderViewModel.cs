using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DotnetSpider.Portal.Models.Spider
{
	public class CreateSpiderViewModel
	{
		/// <summary>
		/// 爬虫名称
		/// </summary>
		[Required]
		[StringLength(255)]
		public string Name { get; set; }

		/// <summary>
		/// docker
		/// </summary>
		[StringLength(255)]
		[Required]
		public string Registry { get; set; }
		
		/// <summary>
		/// docker 镜像仓库名称
		/// </summary>
		[StringLength(255)]
		[Required]
		public string Repository { get; set; }

		/// <summary>
		///  docker 镜像仓库标签
		/// </summary>
		[StringLength(255)]
		[Required]
		public string Tag { get; set; }

		/// <summary>
		/// 任务的 Type
		/// </summary>
		[Required]
		[StringLength(400)]
		public string Type { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[StringLength(255)]
		[Required]
		public string Cron { get; set; }

		/// <summary>
		/// docker 运行的环境变量
		/// </summary>
		[StringLength(255)]
		public string Environment { get; set; }

		public List<string> Tags { get; set; }
	}
}
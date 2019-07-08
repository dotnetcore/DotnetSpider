//using System.ComponentModel.DataAnnotations;
//
//namespace DotnetSpider.Portal.Models.Spider
//{
//	public class AddSpiderModel
//	{
//		/// <summary>
//		/// 爬虫名称
//		/// </summary>
//		[Required]
//		[StringLength(255)]
//		public string Name { get; set; }
//
//		/// <summary>
//		/// docker 镜像仓库地址
//		/// </summary>
//		[StringLength(255)]
//		[Required]
//		public string Registry { get; set; }
//		
//		/// <summary>
//		/// docker 镜像仓库名称
//		/// </summary>
//		[StringLength(255)]
//		[Required]
//		public string Repository { get; set; }
//
//		/// <summary>
//		///  docker 镜像仓库标签
//		/// </summary>
//		[StringLength(255)]
//		[Required]
//		public string Tag { get; set; }
//		
//		[Required]
//		[StringLength(400)]
//		public string Type { get; set; }
//		
//		/// <summary>
//		/// 
//		/// </summary>
//		[StringLength(255)]
//		[Required]
//		public string Cron { get; set; }
//
//		/// <summary>
//		/// docker 运行的环境变量
//		/// </summary>
//		[StringLength(255)]
//		public string Environment { get; set; }
//		
//		/// <summary>
//		/// docker 运行的参数
//		/// </summary>
//		[StringLength(255)]
//		public string Arguments { get; set; }
//	}
//}
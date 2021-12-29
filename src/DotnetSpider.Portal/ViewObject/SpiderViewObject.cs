using System.ComponentModel.DataAnnotations;

namespace DotnetSpider.Portal.ViewObject
{
	public class SpiderViewObject
	{
		/// <summary>
		/// Name
		/// </summary>
		[Required]
		[StringLength(255)]
		public string Name { get; set; }

		/// <summary>
		/// Docker image
		/// </summary>
		[Required]
		[StringLength(255)]
		public string Image { get; set; }

		/// <summary>
		/// 定时表达式
		/// </summary>
		[Required]
		[StringLength(100)]
		public string Cron { get; set; }

		/// <summary>
		/// 环境变量
		/// </summary>
		[StringLength(2000)]
		public string Environment { get; set; }

		/// <summary>
		/// 挂载目录
		/// </summary>
		[StringLength(2000)]
		public string Volume { get; set; }
	}
}

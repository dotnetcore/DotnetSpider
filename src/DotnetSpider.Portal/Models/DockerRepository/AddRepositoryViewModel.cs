using System.ComponentModel.DataAnnotations;

namespace DotnetSpider.Portal.Models.DockerRepository
{
	public class AddRepositoryViewModel
	{
		/// <summary>
		/// 
		/// </summary>
		[Required]
		[StringLength(255, MinimumLength = 4)]
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
		
		[StringLength(255)]
		public string UserName { get; set; }
		
		[StringLength(255)]
		public string Password { get; set; }
	}
}
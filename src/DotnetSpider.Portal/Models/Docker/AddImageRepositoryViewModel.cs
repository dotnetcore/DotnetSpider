using System.ComponentModel.DataAnnotations;

namespace DotnetSpider.Portal.Models.Docker
{
	public class AddImageRepositoryViewModel
	{
		/// <summary>
		/// 
		/// </summary>
		[StringLength(255)]
		public string Name { get; set; }

		/// <summary>
		/// registry.cn-shanghai.aliyuncs.com
		/// </summary>
		[StringLength(255)]
		public string Registry { get; set; }
		
		/// <summary>
		/// 
		/// </summary>
		[StringLength(255)]
		public string Repository { get; set; }
	}
}
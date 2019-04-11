using System.ComponentModel.DataAnnotations;

namespace DotnetSpider.Portal.Models.Spider
{
	public class AddSpiderViewModel
	{
		/// <summary>
		/// 
		/// </summary>
		[Required]
		[StringLength(255)]
		public string Name { get; set; }

		[Required]
		[StringLength(400)]
		public string Class { get; set; }
		
		/// <summary>
		/// 
		/// </summary>
		[StringLength(255)]
		[Required]
		public string Cron { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[StringLength(255)]
		public string Environment { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[StringLength(255)]
		[Required]
		public string Image { get; set; }
		
		/// <summary>
		/// 
		/// </summary>
		public bool Single { get; set; }
	}
}
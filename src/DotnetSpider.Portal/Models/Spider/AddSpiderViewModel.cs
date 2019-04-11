using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
		public string Arguments { get; set; }

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
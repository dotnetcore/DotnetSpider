using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.SqlServer.Query.ExpressionTranslators.Internal;

namespace DotnetSpider.Portal.Entity
{
	public class DockerImageRepository
	{
		public int Id { get; set; }

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

		/// <summary>
		/// Creation time of this entity.
		/// </summary>
		public DateTime CreationTime { get; set; }			
	}
}
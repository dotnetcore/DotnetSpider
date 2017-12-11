using DotnetSpider.Core.Infrastructure.Database;
using System.Collections.Generic;

namespace DotnetSpider.Extension.Pipeline
{
	public class HttpPipelinePackage
	{
		/// <summary>
		/// Sql
		/// </summary>
		public string Sql { get; set; }

		public Database D { get; set; }

		public List<object> Dt { get; set; }
	}
}

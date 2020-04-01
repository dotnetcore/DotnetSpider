using System;
using Microsoft.Extensions.Configuration;

namespace DotnetSpider.Portal
{
	public class PortalOptions
	{
		private readonly IConfiguration _configuration;

		public PortalOptions(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public string DatabaseType => _configuration["DatabaseType"];

		/// <summary>
		/// 数据库连接字符串
		/// </summary>
		public string ConnectionString => _configuration["ConnectionString"];

		public string Docker => _configuration["Docker"];

		public string[] DockerVolumes =>
			string.IsNullOrWhiteSpace(_configuration["DockerVolumes"])
				? new string[0]
				: _configuration["DockerVolumes"].Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);
	}
}

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

		public string Database => _configuration["Database"];

		/// <summary>
		/// 数据库连接字符串
		/// </summary>
		public string ConnectionString => _configuration["ConnectionString"];

		public string Docker => _configuration["Docker"];

		public string DockerVolumes => _configuration["DockerVolumes"];
	}
}
using System;
using DotnetSpider.DataFlow.Storage;
using Microsoft.Extensions.Configuration;

namespace DotnetSpider.PostgreSql
{
	public class PostgreOptions
	{
		private readonly IConfiguration _configuration;

		public PostgreOptions(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public StorageMode Mode => string.IsNullOrWhiteSpace(_configuration["Postgre:Mode"])
			? StorageMode.Insert
			: (StorageMode)Enum.Parse(typeof(StorageMode), _configuration["Postgre:Mode"]);

		/// <summary>
		/// 数据库连接字符串
		/// </summary>
		public string ConnectionString => _configuration["Postgre:ConnectionString"];

		/// <summary>
		/// 数据库操作重试次数
		/// </summary>
		public int RetryTimes => string.IsNullOrWhiteSpace(_configuration["Postgre:RetryTimes"])
			? 600
			: int.Parse(_configuration["Postgre:RetryTimes"]);

		/// <summary>
		/// 是否使用事务操作。默认不使用。
		/// </summary>
		public bool UseTransaction => !string.IsNullOrWhiteSpace(_configuration["Postgre:UseTransaction"]) &&
		                              bool.Parse(_configuration["Postgre:UseTransaction"]);

		/// <summary>
		/// 数据库忽略大小写
		/// </summary>
		public bool IgnoreCase => !string.IsNullOrWhiteSpace(_configuration["Postgre:IgnoreCase"]) &&
		                          bool.Parse(_configuration["Postgre:IgnoreCase"]);
	}
}

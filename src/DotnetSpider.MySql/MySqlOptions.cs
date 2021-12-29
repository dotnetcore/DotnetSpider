using System;
using DotnetSpider.DataFlow.Storage;
using Microsoft.Extensions.Configuration;

namespace DotnetSpider.MySql
{
	public class MySqlOptions
	{
		private readonly IConfiguration _configuration;

		public MySqlOptions(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public StorageMode Mode => string.IsNullOrWhiteSpace(_configuration["MySql:Mode"])
			? StorageMode.Insert
			: (StorageMode)Enum.Parse(typeof(StorageMode), _configuration["MySql:Mode"]);

		/// <summary>
		/// 数据库连接字符串
		/// </summary>
		public string ConnectionString => _configuration["MySql:ConnectionString"];

		/// <summary>
		/// 数据库操作重试次数
		/// </summary>
		public int RetryTimes => string.IsNullOrWhiteSpace(_configuration["MySql:RetryTimes"])
			? 600
			: int.Parse(_configuration["MySql:RetryTimes"]);

		/// <summary>
		/// 是否使用事务操作。默认不使用。
		/// </summary>
		public bool UseTransaction => !string.IsNullOrWhiteSpace(_configuration["MySql:UseTransaction"]) &&
		                              bool.Parse(_configuration["MySql:UseTransaction"]);

		/// <summary>
		/// 数据库忽略大小写
		/// </summary>
		public bool IgnoreCase => !string.IsNullOrWhiteSpace(_configuration["MySql:IgnoreCase"]) &&
		                          bool.Parse(_configuration["MySql:IgnoreCase"]);
	}
}

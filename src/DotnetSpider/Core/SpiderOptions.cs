using System;
using DotnetSpider.Data.Storage;
using Microsoft.Extensions.Configuration;

namespace DotnetSpider.Core
{
    public interface ISpiderOptions
    {
        string ConnectionString { get; }

        string Storage { get; }

        StorageType StorageType { get; }

        string MySqlFileType { get; }

        bool IgnoreCase { get; }

        int StorageRetryTimes { get; }

        /// <summary>
        /// 是否使用事务操作。默认不使用。
        /// </summary>
        bool StorageUseTransaction { get; }

        string EmailHost { get; }

        string EmailAccount { get; }

        string EmailPassword { get; }

        string EmailDisplayName { get; }

        string EmailPort { get; }
    }


    public class SpiderOptions : ISpiderOptions
    {
        private readonly IConfiguration _configuration;

        public string ConnectionString => _configuration["ConnectionString"];

        public string Storage => _configuration["Storage"];

        public bool IgnoreCase => string.IsNullOrWhiteSpace(_configuration["IgnoreCase"]) ||
                                  bool.Parse(_configuration["IgnoreCase"]);

        public int StorageRetryTimes => string.IsNullOrWhiteSpace(_configuration["StorageRetryTimes"])
            ? 600
            : int.Parse(_configuration["StorageRetryTimes"]);

        public bool StorageUseTransaction => !string.IsNullOrWhiteSpace(_configuration["StorageUseTransaction"]) &&
                                             bool.Parse(_configuration["StorageUseTransaction"]);

        public StorageType StorageType => string.IsNullOrWhiteSpace(_configuration["StorageType"])
            ? StorageType.InsertIgnoreDuplicate
            : (StorageType) Enum.Parse(typeof(StorageType), _configuration["StorageType"]);

        public string MySqlFileType => _configuration["MySqlFileType"];

        public string EmailHost => _configuration["EmailHost"];

        public string EmailAccount => _configuration["EmailAccount"];

        public string EmailPassword => _configuration["EmailPassword"];

        public string EmailDisplayName => _configuration["EmailDisplayName"];

        public string EmailPort => _configuration["EmailPort"];

        public SpiderOptions(IConfiguration configuration)
        {
            _configuration = configuration;
        }
    }
}
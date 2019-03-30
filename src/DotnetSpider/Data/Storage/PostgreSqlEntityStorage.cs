using DotnetSpider.Core;

namespace DotnetSpider.Data.Storage
{
    public class PostgreSqlEntityStorage : MySqlEntityStorage
    {
        public new static PostgreSqlEntityStorage CreateFromOptions(ISpiderOptions options)
        {
            return new PostgreSqlEntityStorage(options.StorageType, options.ConnectionString)
            {
                IgnoreCase = options.IgnoreCase,
                RetryTimes = options.StorageRetryTimes,
                UseTransaction = options.StorageUseTransaction
            };
        }
        
        public PostgreSqlEntityStorage(StorageType storageType = StorageType.InsertIgnoreDuplicate,
            string connectionString = null) : base(storageType,
            connectionString)
        {
        }
    }
}
using DotnetSpider.Core;

namespace DotnetSpider.Data.Storage
{
	/// <summary>
	/// PostgreSql 保存解析(实体)结果
	/// </summary>
    public class PostgreSqlEntityStorage : MySqlEntityStorage
    {
	    /// <summary>
	    /// 根据配置返回存储器
	    /// </summary>
	    /// <param name="options">配置</param>
	    /// <returns></returns>
        public new static PostgreSqlEntityStorage CreateFromOptions(ISpiderOptions options)
        {
            return new PostgreSqlEntityStorage(options.StorageType, options.StorageConnectionString)
            {
                IgnoreCase = options.StorageIgnoreCase,
                RetryTimes = options.StorageRetryTimes,
                UseTransaction = options.StorageUseTransaction
            };
        }
        
	    /// <summary>
	    /// 构造方法
	    /// </summary>
	    /// <param name="storageType">存储器类型</param>
	    /// <param name="connectionString">连接字符串</param>
        public PostgreSqlEntityStorage(StorageType storageType = StorageType.InsertIgnoreDuplicate,
            string connectionString = null) : base(storageType,
            connectionString)
        {
        }
    }
}
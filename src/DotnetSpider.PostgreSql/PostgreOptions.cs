using System;
using DotnetSpider.DataFlow.Storage.Entity;
using Microsoft.Extensions.Configuration;

namespace DotnetSpider.PostgreSql;

public class PostgreOptions(IConfiguration configuration)
{
    public StorageMode Mode => string.IsNullOrWhiteSpace(configuration["Postgre:Mode"])
        ? StorageMode.Insert
        : (StorageMode)Enum.Parse(typeof(StorageMode), configuration["Postgre:Mode"]);

    /// <summary>
    /// 数据库连接字符串
    /// </summary>
    public string ConnectionString => configuration["Postgre:ConnectionString"];

    /// <summary>
    /// 数据库操作重试次数
    /// </summary>
    public int RetryTimes => string.IsNullOrWhiteSpace(configuration["Postgre:RetryTimes"])
        ? 600
        : int.Parse(configuration["Postgre:RetryTimes"]);

    /// <summary>
    /// 是否使用事务操作。默认不使用。
    /// </summary>
    public bool UseTransaction => !string.IsNullOrWhiteSpace(configuration["Postgre:UseTransaction"]) &&
                                  bool.Parse(configuration["Postgre:UseTransaction"]);

    /// <summary>
    /// 数据库忽略大小写
    /// </summary>
    public bool IgnoreCase => !string.IsNullOrWhiteSpace(configuration["Postgre:IgnoreCase"]) &&
                              bool.Parse(configuration["Postgre:IgnoreCase"]);
}
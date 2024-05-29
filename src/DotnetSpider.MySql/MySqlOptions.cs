using System;
using DotnetSpider.DataFlow.Storage.Entity;
using Microsoft.Extensions.Configuration;

namespace DotnetSpider.MySql;

public class MySqlOptions(IConfiguration configuration)
{
    public StorageMode Mode => string.IsNullOrWhiteSpace(configuration["MySql:Mode"])
        ? StorageMode.Insert
        : (StorageMode)Enum.Parse(typeof(StorageMode), configuration["MySql:Mode"]);

    /// <summary>
    /// 数据库连接字符串
    /// </summary>
    public string ConnectionString => configuration["MySql:ConnectionString"];

    /// <summary>
    /// 数据库操作重试次数
    /// </summary>
    public int RetryTimes => string.IsNullOrWhiteSpace(configuration["MySql:RetryTimes"])
        ? 600
        : int.Parse(configuration["MySql:RetryTimes"]);

    /// <summary>
    /// 是否使用事务操作。默认不使用。
    /// </summary>
    public bool UseTransaction => !string.IsNullOrWhiteSpace(configuration["MySql:UseTransaction"]) &&
                                  bool.Parse(configuration["MySql:UseTransaction"]);

    /// <summary>
    /// 数据库忽略大小写
    /// </summary>
    public bool IgnoreCase => !string.IsNullOrWhiteSpace(configuration["MySql:IgnoreCase"]) &&
                              bool.Parse(configuration["MySql:IgnoreCase"]);
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using DotnetSpider.Core;
using DotnetSpider.Downloader;
using MySql.Data.MySqlClient;

namespace DotnetSpider.RequestSupply
{
    /// <summary>
    /// 基于关系型数据库的请求入队
    /// </summary>
    public class RelationalDatabaseRequestSupply : IRequestSupplier
    {
        private readonly string _sql;
        private readonly Database _database;
        private readonly string _connectionString;

        /// <summary>
        /// 拼接Url的方式, 会把Columns对应列的数据传入
        /// https://s.taobao.com/search?q={0},s=0;
        /// </summary>
        private readonly string[] _formats;

        private readonly string[] _formatArguments;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="database">数据库类型</param>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="sql">SQL 语句</param>
        /// <param name="formatArguments">起始链接格式化参数</param>
        /// <param name="formats">起始链接格式化模版</param>
        public RelationalDatabaseRequestSupply(Database database, string connectionString, string sql,
            string[] formatArguments,
            params string[] formats)
        {
            _database = database;
            _connectionString = connectionString;

            _sql = sql;
            _formats = formats;
            _formatArguments = formatArguments;
        }

        /// <summary>
        /// 从数据库中查询出的结果可以先做一下格式
        /// </summary>
        /// <param name="item">数据对象</param>
        protected virtual void FormatDataObject(IDictionary<string, string> item)
        {
        }

        /// <summary>
        /// 格式化最终的请求信息
        /// </summary>
        /// <param name="request">请求信息</param>
        protected virtual void FormatRequest(Request request)
        {
        }

        /// <summary>
        /// 运行请求供应
        /// </summary>
        /// <param name="enqueueAction">请求入队的方法</param>
        public void Run(Action<Request> enqueueAction)
        {
            using (var conn = CreateConnection())
            {
                foreach (var data in conn.Query(_sql))
                {
                    var dic =
                        ((IDictionary<string, string>) data).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                    FormatDataObject(dic);

                    object[] arguments = _formatArguments.Select(a => dic[a]).Select(x => (object) x).ToArray();

                    foreach (var format in _formats)
                    {
                        var url = string.Format(format, arguments);
                        var request = new Request
                        {
                            Url = url
                        };
                        request.AddProperty(dic);
                        FormatRequest(request);
                        enqueueAction(request);
                    }
                }
            }
        }

        private IDbConnection CreateConnection()
        {
            switch (_database)
            {
                case Database.MySql:
                case Database.PostgreSql:
                {
                    return new MySqlConnection(_connectionString);
                }
                case Database.SqlServer:
                {
                    return new SqlConnection(_connectionString);
                }
            }

            throw new NotImplementedException($"暂未支持数据库: {_database}");
        }
    }
}
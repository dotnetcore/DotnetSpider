using System.Collections.Generic;
using System.Linq;
using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure.Database;
using System.Configuration;
using System.Runtime.CompilerServices;
using DotnetSpider.Extension.Infrastructure;
using DotnetSpider.Common;

[assembly: InternalsVisibleTo("DotnetSpider.Extension.Test")]
namespace DotnetSpider.Extension
{
	/// <summary>
	/// 起始链接构造器
	/// </summary>
	public class DbRequestBuilder : RequestBuilder
	{
		private readonly ConnectionStringSettings _connectionStringSettings;

		private readonly string _sql;

		/// <summary>
		/// 拼接Url的方式, 会把Columns对应列的数据传入
		/// https://s.taobao.com/search?q={0},s=0;
		/// </summary>
		private readonly string[] _formateStrings;

		private readonly string[] _formateArguments;

		/// <summary>
		/// 从数据库中查询出的结果可以先做一下格式
		/// </summary>
		/// <param name="item">数据对象</param>
		protected virtual void FormateDataObject(IDictionary<string, object> item)
		{
		}

		/// <summary>
		/// 格式化最终的请求信息
		/// </summary>
		/// <param name="request">请求信息</param>
		protected virtual void FormateRequest(Request request)
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="sql">SQL 语句</param>
		/// <param name="formateArguments">起始链接格式化参数</param>
		/// <param name="formateStrings">起始链接格式化模版</param>
		public DbRequestBuilder(string sql, string[] formateArguments, params string[] formateStrings)
		{
			if (Env.DataConnectionStringSettings == null)
			{
				throw new SpiderException("DataConnection is unfound in app.config");
			}
			_connectionStringSettings = Env.DataConnectionStringSettings;
			_sql = sql;
			_formateStrings = formateStrings;
			_formateArguments = formateArguments;
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="source">数据库类型</param>
		/// <param name="connectString">数据库连接字符串</param>
		/// <param name="sql">SQL 语句</param>
		/// <param name="formateArguments">起始链接格式化参数</param>
		/// <param name="formateStrings">起始链接格式化模版</param>
		public DbRequestBuilder(Database source, string connectString, string sql, string[] formateArguments, params string[] formateStrings)
		{
			switch (source)
			{
				case Database.MySql:
					{
						_connectionStringSettings = new ConnectionStringSettings("MySql", connectString, "MySql.Data.MySqlClient");
						break;
					}
				case Database.SqlServer:
					{
						_connectionStringSettings = new ConnectionStringSettings("SqlServer", connectString, "System.Data.SqlClient");
						break;
					}
				default:
					{
						throw new SpiderException($"Database {source} is unsported right now");
					}
			}
			_sql = sql;
			_formateStrings = formateStrings;
			_formateArguments = formateArguments;
		}

		/// <summary>
		/// 查询数据库结果
		/// </summary>
		/// <returns>数据库结果</returns>
		protected List<Dictionary<string, dynamic>> QueryDatas()
		{
			List<Dictionary<string, dynamic>> list = new List<Dictionary<string, dynamic>>();
			using (var conn = _connectionStringSettings.CreateDbConnection())
			{
				foreach (var item in conn.MyQuery(_sql))
				{
					var dataItem = item as Dictionary<string, dynamic>;
					FormateDataObject(dataItem);
					list.Add(dataItem);
				}
			}
			return list;
		}

		/// <summary>
		/// 构造起始链接对象并添加到网站信息对象中
		/// </summary>
		/// <param name="site">网站信息</param>
		public override void Build(Site site)
		{
			var datas = QueryDatas();

			foreach (var data in datas)
			{
				object[] arguments = _formateArguments.Select(a => data[a]).ToArray();

				foreach (var formate in _formateStrings)
				{
					string url = string.Format(formate, arguments);
					var request = new Request(url, data);
					FormateRequest(request);
					site.AddRequests(request);
				}
			}
		}
	}
}

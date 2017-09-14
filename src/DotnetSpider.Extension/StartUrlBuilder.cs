using System.Collections.Generic;
using System.Linq;
using DotnetSpider.Core;
using DotnetSpider.Core.Redial;
using Dapper;
using DotnetSpider.Core.Infrastructure.Database;
using System.Configuration;

namespace DotnetSpider.Extension
{
	public class DbStartUrlBuilder : StartUrlBuilder
	{
		public ConnectionStringSettings ConnectionStringSettings { get; }

		public string Sql { get; }

		/// <summary>
		/// 拼接Url的方式, 会把Columns对应列的数据传入
		/// https://s.taobao.com/search?q={0},s=0;
		/// </summary>
		public string[] FormateStrings { get; }

		public string[] FormateArguments { get; }

		protected virtual void FormateDataObject(IDictionary<string, object> item)
		{
		}

		protected virtual void FormateRequest(Request request)
		{
		}

		public DbStartUrlBuilder(string sql, string[] formateArguments, params string[] formateStrings)
		{
			if (Env.DataConnectionStringSettings == null)
			{
				throw new SpiderException("DataConnection is unfound in app.config.");
			}
			ConnectionStringSettings = Env.DataConnectionStringSettings;
			Sql = sql;
			FormateStrings = formateStrings;
			FormateArguments = formateArguments;
		}

		public DbStartUrlBuilder(Database source, string connectString, string sql, string[] formateArguments, params string[] formateStrings)
		{
			switch (source)
			{
				case Database.MySql:
					{
						ConnectionStringSettings = new ConnectionStringSettings("MySql", connectString, "MySql.Data.MySqlClient");
						break;
					}
				case Database.SqlServer:
					{
						ConnectionStringSettings = new ConnectionStringSettings("SqlServer", connectString, "System.Data.SqlClient");
						break;
					}
				default:
					{
						throw new SpiderException($"Database {source} is unsported right now.");
					}
			}
			Sql = sql;
			FormateStrings = formateStrings;
			FormateArguments = formateArguments;
		}

		protected List<IDictionary<string, object>> QueryDatas()
		{
			List<IDictionary<string, object>> list = new List<IDictionary<string, object>>();
			NetworkCenter.Current.Execute("dbsb", () =>
			{
				using (var conn = ConnectionStringSettings.GetDbConnection())
				{
					foreach (var item in conn.Query(Sql))
					{
						var dataItem = item as IDictionary<string, object>;
						FormateDataObject(dataItem);
						list.Add(dataItem);
					}
				}
			});
			return list;
		}

		public override void Build(Site site)
		{
			var datas = QueryDatas();

			foreach (var data in datas)
			{
				object[] arguments = FormateArguments.Select(a => data[a]).ToArray();

				foreach (var formate in FormateStrings)
				{
					string url = string.Format(formate, arguments);
					var request = new Request(url, data);
					FormateRequest(request);
					site.AddStartRequest(request);
				}
			}
		}
	}
}

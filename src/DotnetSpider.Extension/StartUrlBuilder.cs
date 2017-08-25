using System.Collections.Generic;
using System.Linq;
using DotnetSpider.Core;
using DotnetSpider.Extension.Infrastructure;
using DotnetSpider.Core.Redial;
using Dapper;
#if !NET_CORE

#else
using System.Net;
#endif

namespace DotnetSpider.Extension
{

	public class DbStartUrlBuilder : StartUrlBuilder
	{
		public DataSource Source { get; }

		public string ConnectString { get; }

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

		public DbStartUrlBuilder(DataSource source, string connectString, string sql, string[] formateArguments, params string[] formateStrings)
		{
			Source = source;
			ConnectString = connectString;
			Sql = sql;
			FormateStrings = formateStrings;
			FormateArguments = formateArguments;
		}

		protected List<IDictionary<string, object>> QueryDatas()
		{
			List<IDictionary<string, object>> list = new List<IDictionary<string, object>>();
			NetworkCenter.Current.Execute("dbsb", () =>
			{
				using (var conn = DataSourceUtils.GetConnection(Source, ConnectString))
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

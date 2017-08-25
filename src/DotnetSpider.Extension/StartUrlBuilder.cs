using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DotnetSpider.Core;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using DotnetSpider.Extension.Infrastructure;
using DotnetSpider.Core.Redial;
using Dapper;
#if !NET_CORE
using System.Web;
#else
using System.Net;
#endif

namespace DotnetSpider.Extension
{

	public class DbStartUrlBuilder : StartUrlBuilder
	{
		public DataSource Source { get; private set; }

		public string ConnectString { get; private set; }

		public string Sql { get; private set; }

		/// <summary>
		/// 拼接Url的方式, 会把Columns对应列的数据传入
		/// https://s.taobao.com/search?q={0},s=0;
		/// </summary>
		public string[] FormateStrings { get; private set; }

		public string[] FormateArguments { get; private set; }

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
				using (var conn = DataSourceUtil.GetConnection(Source, ConnectString))
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

			HashSet<Request> results = new HashSet<Request>();
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

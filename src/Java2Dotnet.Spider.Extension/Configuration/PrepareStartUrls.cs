using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Extension.Model.Formatter;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Data.Common;
#if !NET_CORE
using System.Web;
#else
using System.Net;
#endif

namespace Java2Dotnet.Spider.Extension.Configuration
{
	public enum DataSource
	{
		MySql,
		MsSql
	}

	public class DataSourceUtil
	{
		public static DbConnection GetConnection(DataSource source, string connectString)
		{
			switch (source)
			{
				case DataSource.MySql:
					{
						return new MySqlConnection(connectString);
					}
				case DataSource.MsSql:
					{
						return new SqlConnection(connectString);
					}
			}

			throw new SpiderExceptoin($"Unsported datasource: {source}");
		}
	}

	public abstract class PrepareStartUrls
	{
		[Flags]
		public enum Types
		{
			GeneralDb,
			DbCycle,
			Cycle,
			LinkSpider
		}

		public string Method { get; set; } = "GET";

		public string Referer { get; set; }

		public string PostBody { get; set; }

		public string Origin { get; set; }

		public abstract Types Type { get; internal set; }

		public abstract void Build(Site site, dynamic obj);
	}

	public class DbPrepareStartUrls : PrepareStartUrls
	{
		public class Column
		{
			public string Name { get; set; }

			public List<Formatter> Formatters { get; set; } = new List<Formatter>();
		}

		public override Types Type { get; internal set; } = Types.GeneralDb;

		public DataSource Source { get; set; } = DataSource.MySql;

		public string ConnectString { get; set; }

		public string GroupBy { get; set; }

		public string OrderBy { get; set; }

		/// <summary>
		/// 数据来源表名, 需要Schema/数据库名
		/// </summary>
		public string TableName { get; set; }

		/// <summary>
		/// 对表的筛选
		/// 如: cdate='2016-03-01', isUsed=true
		/// </summary>
		public List<string> Filters { get; set; }

		/// <summary>
		/// 用于拼接Url所需要的列
		/// </summary>
		public List<Column> Columns { get; set; } = new List<Column>();

		public int Limit { get; set; }

		/// <summary>
		/// 拼接Url的方式, 会把Columns对应列的数据传入
		/// https://s.taobao.com/search?q={0},s=0;
		/// </summary>
		public List<string> FormateStrings { get; set; }

		public override void Build(Site site, dynamic obj)
		{
			using (var conn = DataSourceUtil.GetConnection(Source, ConnectString))
			{
				List<Dictionary<string, object>> datas = new List<Dictionary<string, object>>();
				string sql = GetSelectQueryString();
				conn.Open();
				var command = conn.CreateCommand();
				command.CommandText = sql;
				command.CommandType = CommandType.Text;

				var reader = command.ExecuteReader();

				while (reader.Read())
				{
					Dictionary<string, object> data = new Dictionary<string, object>();
					int count = reader.FieldCount;
					for (int i = 0; i < count; ++i)
					{
						string name = reader.GetName(i);
						data.Add(name, reader.GetValue(i));
					}

					List<string> arguments = new List<string>();
					foreach (var column in Columns)
					{
						string value = data[column.Name]?.ToString();

						foreach (var formatter in column.Formatters)
						{
							value = formatter.Formate(value);
						}
						arguments.Add(value);
					}

					foreach (var formate in FormateStrings)
					{
						string tmpUrl = string.Format(formate, arguments.Cast<object>().ToArray());
						site.AddStartRequest(new Request(tmpUrl, 0, data)
						{
							Method = Method,
							Origin = Origin,
							PostBody = GetPostBody(PostBody, data),
							Referer = Referer
						});
					}
				}
#if NET_CORE && TEST
				foreach (var data in datas)
				{
					foreach (var entry in data)
					{
						Console.WriteLine(entry.Key + ":" + entry.Value);
					}
				}
#endif

#if !NET_CORE
				reader.Close();
#else
				reader.Dispose();
#endif
			}
		}

		public static string GetPostBody(string postBody, Dictionary<string, object> datas)
		{
			if (string.IsNullOrEmpty(postBody))
			{
				return null;
			}

			Regex regex = new Regex(@"__URLENCODE\('(\w|\d)+'\)");
			string tmpPostBody = postBody;
			foreach (Match match in regex.Matches(postBody))
			{
				string tmp = match.Value;
				int startIndex = tmp.IndexOf("__URLENCODE('");
				int endIndex = tmp.IndexOf("')", startIndex);
				string arg = tmp.Substring(startIndex + 13, endIndex - startIndex - 13);
#if !NET_CORE
				var value = HttpUtility.UrlEncode(datas[arg].ToString());
#else
				var value = WebUtility.UrlEncode(datas[arg].ToString());
#endif
				tmpPostBody = postBody.Replace(tmp, value);
			}

			// implement more rules
			return tmpPostBody;
		}

		protected string GetSelectQueryString()
		{
			switch (Source)
			{
				case DataSource.MySql:
					{
						StringBuilder builder = new StringBuilder($"SELECT * FROM {TableName}");
						if (Filters != null && Filters.Count > 0)
						{
							builder.Append(" WHERE " + Filters.First());
							if (Filters.Count > 1)
							{
								for (int i = 1; i < Filters.Count; ++i)
								{
									builder.Append(" AND " + Filters[i]);
								}
							}
						}

						if (!string.IsNullOrEmpty(GroupBy))
						{
							builder.Append($" {GroupBy} ");
						}

						if (!string.IsNullOrEmpty(OrderBy))
						{
							builder.Append($" {OrderBy} ");
						}

						if (Limit > 0)
						{
							builder.Append($" LIMIT {Limit} ");
						}


						return builder.ToString();
					}
			}
			throw new SpiderExceptoin($"Unsport Source: {Source}");
		}
	}

	public class CyclePrepareStartUrls : PrepareStartUrls
	{
		public override Types Type { get; internal set; } = Types.Cycle;

		public int From { get; set; }
		public int To { get; set; }

		public string FormateString { get; set; }

		public override void Build(Site site, dynamic obj)
		{
			for (int i = From; i <= To; ++i)
			{
				site.AddStartUrl(string.Format(FormateString, i));
			}
		}
	}

	public class DbCyclePrepareStartUrls : DbPrepareStartUrls
	{
		public override Types Type { get; internal set; } = Types.DbCycle;

		public int Interval { get; set; }
		public string ColumnSeparator { get; set; }
		public string RowSeparator { get; set; }

		public string FormateString { get; set; }

		public override void Build(Site site, dynamic obj)
		{
			using (var conn = DataSourceUtil.GetConnection(Source, ConnectString))
			{
				//List<Dictionary<string, object>> datas = new List<Dictionary<string, object>>();
				string sql = GetSelectQueryString();
				conn.Open();
				var command = conn.CreateCommand();
				command.CommandText = sql;
				command.CommandType = CommandType.Text;

				var reader = command.ExecuteReader();
				int interval = 0;
				StringBuilder formatBuilder = new StringBuilder();
				while (reader.Read())
				{
					Dictionary<string, object> data = new Dictionary<string, object>();
					int count = reader.FieldCount;
					for (int i = 0; i < count; ++i)
					{
						string name = reader.GetName(i);
						data.Add(name, reader.GetValue(i));
					}

					if (interval == Interval)
					{
						foreach (var formate in FormateStrings)
						{
							string tmpUrl = string.Format(formate, formatBuilder.ToString(0, formatBuilder.Length - (string.IsNullOrEmpty(RowSeparator) ? 0 : RowSeparator.Length)));
							site.AddStartRequest(new Request(tmpUrl, 0, null)
							{
								Method = Method,
								Origin = Origin,
								Referer = Referer
							});
						}

						interval = 0;
						formatBuilder = new StringBuilder();
					}

					Dictionary<string, object> tmp = data;

					StringBuilder argumentsBuilder = new StringBuilder();
					foreach (var column in Columns)
					{
						string value = tmp[column.Name]?.ToString();

						value = column.Formatters.Aggregate(value, (current, formatter) => formatter.Formate(current));

						argumentsBuilder.Append(value).Append(ColumnSeparator);
					}
					formatBuilder.Append(argumentsBuilder.ToString(0, argumentsBuilder.Length - (string.IsNullOrEmpty(ColumnSeparator) ? 0 : ColumnSeparator.Length))).Append(RowSeparator);
					interval++;
					//datas.Add(values);
				}
 
				if (interval != 0)
				{
					foreach (var formate in FormateStrings)
					{
						string tmpUrl = string.Format(formate, formatBuilder.ToString(0, formatBuilder.Length - 1));
						site.AddStartRequest(new Request(tmpUrl, 0, null)
						{
							Method = Method,
							Origin = Origin,
							Referer = Referer
						});
					}

					interval = 0;
					formatBuilder = new StringBuilder();
				}

#if !NET_CORE
				reader.Close();
#else
				reader.Dispose();
#endif
			}
		}
	}

	public class LinkSpiderPrepareStartUrls : PrepareStartUrls
	{
		public override Types Type { get; internal set; } = Types.LinkSpider;

		/// <summary>
		/// 用于拼接Url所需要的列
		/// </summary>
		public List<DbPrepareStartUrls.Column> Columns { get; set; } = new List<DbPrepareStartUrls.Column>();

		/// <summary>
		/// 拼接Url的方式, 会把Columns对应列的数据传入
		/// https://s.taobao.com/search?q={0},s=0;
		/// </summary>
		public List<string> FormateStrings { get; set; }

		public override void Build(Site site, dynamic obj)
		{
			foreach (JObject jobject in obj)
			{
				Dictionary<string, object> tmp = obj;
				foreach (var node in jobject.Children())
				{
					tmp.Add("", node.ToString());
				}

				List<string> arguments = new List<string>();
				foreach (var column in Columns)
				{
					string value = tmp[column.Name]?.ToString();

					value = column.Formatters.Aggregate(value, (current, formatter) => formatter.Formate(current));
					arguments.Add(value);
				}

				foreach (var formate in FormateStrings)
				{
					string tmpUrl = string.Format(formate, arguments.Cast<object>().ToArray());
					site.AddStartRequest(new Request(tmpUrl, 0, tmp)
					{
						Method = Method,
						Origin = Origin,
						PostBody = DbPrepareStartUrls.GetPostBody(PostBody, tmp),
						Referer = Referer
					});
				}
			}
		}
	}
}

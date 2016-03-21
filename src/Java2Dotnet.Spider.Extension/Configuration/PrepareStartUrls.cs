using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Extension.Model;
using Java2Dotnet.Spider.Extension.Model.Formatter;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;

namespace Java2Dotnet.Spider.Extension.Configuration
{
	public abstract class PrepareStartUrls : IJobject
	{
		public enum Types
		{
			GeneralDb,
			Cycle
		}

		public abstract Types Type { get; internal set; }

		public abstract void Build(Site site);
	}

	public class GeneralDbPrepareStartUrls : PrepareStartUrls
	{
		public class Column
		{
			public string Name { get; set; }
			public List<JObject> Formatters { get; set; }
		}

		public enum DataSource
		{
			MySql,
			MsSql
		}

		public override Types Type { get; internal set; } = Types.GeneralDb;

		public DataSource Source { get; set; }

		public string ConnectString { get; set; }

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
		public List<Column> Columns { get; set; }

		public int Limit { get; set; }

		/// <summary>
		/// 拼接Url的方式, 会把Columns对应列的数据传入
		/// https://s.taobao.com/search?q={0},s=0;
		/// </summary>
		public string FormateString { get; set; }

		public override void Build(Site site)
		{
			Dictionary<Column, List<Formatter>> dic = new Dictionary<Column, List<Formatter>>();
			foreach (var column in Columns)
			{
				List<Formatter> formatters = EntityExtractor.GenerateFormatter(column.Formatters);
				dic.Add(column, formatters);
			}

			using (var conn = new MySqlConnection(ConnectString))
			{
				var data = conn.Query(GetSelectQueryString());

				Parallel.ForEach(data, new ParallelOptions { MaxDegreeOfParallelism = 1 }, brand =>
				{
					IDictionary<string, object> tmp = (IDictionary<string, object>)brand;
					List<string> arguments = new List<string>();
					foreach (var column in Columns)
					{
						string value = tmp[column.Name]?.ToString();

						foreach (var formatter in dic[column])
						{
							value = formatter.Formate(value);
						}
						arguments.Add(value);
					}

					string tmpUrl = string.Format(FormateString, arguments.Cast<object>().ToArray());


					site.AddStartUrl(tmpUrl, tmp);
				});
			}
		}

		private string GetSelectQueryString()
		{
			switch (Source)
			{
				case DataSource.MySql:
					{
						return $"select * from {TableName} {(Filters == null || Filters.Count == 0 ? "" : "where " + Filters.Select(f => $"and {f}"))} " + (Limit > 0 ? $"limit {Limit}" : "");
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

		public override void Build(Site site)
		{
			for (int i = 1; i <= 50; ++i)
			{
				site.AddStartUrl(string.Format(FormateString, i));
			}
		}
	}
}

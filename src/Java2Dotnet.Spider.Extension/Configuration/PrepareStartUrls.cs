using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Extension.Model;
using Java2Dotnet.Spider.Extension.Model.Formatter;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Java2Dotnet.Spider.Extension.Configuration
{
	public abstract class PrepareStartUrls
	{
		[Flags]
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

			public List<Formatter> Formatters { get; set; } = new List<Formatter>();
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
		public List<Column> Columns { get; set; } = new List<Column>();

		public int Limit { get; set; }

		/// <summary>
		/// 拼接Url的方式, 会把Columns对应列的数据传入
		/// https://s.taobao.com/search?q={0},s=0;
		/// </summary>
		public string FormateString { get; set; }

		public override void Build(Site site)
		{
			using (var conn = new MySqlConnection(ConnectString))
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
					Dictionary<string, object> values = new Dictionary<string, object>();
					int count = reader.FieldCount;
					for (int i = 0; i < count; ++i)
					{
						string name = reader.GetName(i);
						values.Add(name, reader.GetValue(i));
					}
					datas.Add(values);
				}

				reader.Close();

				Parallel.ForEach(datas, new ParallelOptions { MaxDegreeOfParallelism = 1 }, brand =>
				{
					Dictionary<string, object> tmp = brand;
					List<string> arguments = new List<string>();
					foreach (var column in Columns)
					{
						string value = tmp[column.Name]?.ToString();

						foreach (var formatter in column.Formatters)
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

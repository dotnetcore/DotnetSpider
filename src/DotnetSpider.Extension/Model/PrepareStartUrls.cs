using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DotnetSpider.Core;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using DotnetSpider.Extension.Infrastructure;
using DotnetSpider.Core.Infrastructure;
#if !NET_CORE
using System.Web;
#else
using System.Net;
#endif

namespace DotnetSpider.Extension.Model
{
	public class DataColumn
	{
		public string Name { get; set; }

		public Formatter.Formatter[] Formatters { get; set; }
	}

	public abstract class PrepareStartUrls : Named
	{
		public string Method { get; set; } = "GET";

		public string Referer { get; set; }

		public string PostBody { get; set; }

		public string Origin { get; set; }

		public Dictionary<string, object> Extras { get; set; }

		public abstract void Build(Spider site, dynamic obj);
	}

	public class CyclePrepareStartUrls : PrepareStartUrls
	{
		public int From { get; set; }
		public int To { get; set; }
		public int PostFrom { get; set; }
		public int PostTo { get; set; }

		public int Interval { get; set; } = 1;
		public int PostInterval { get; set; } = 1;

		public string IndexKey { get; set; } = "IndexKey";
		public string PostIndexKey { get; set; } = "PostIndexKey";

		public string FormateString { get; set; }

		public override void Build(Spider spider, dynamic obj)
		{
			Dictionary<string, object> data = new Dictionary<string, object>();

			if (Extras != null)
			{
				foreach (var extra in Extras)
				{
					data.Add(extra.Key, extra.Value);
				}
			}

			data.Add(IndexKey, null);
			data.Add(PostIndexKey, null);

			for (int i = From; i <= To; i += Interval)
			{
				data[IndexKey] = i;
				for (int j = PostFrom; j <= PostFrom; j += PostInterval)
				{
					data[PostIndexKey] = j;
					spider.Scheduler.Push(new Request(string.Format(FormateString, i), data)
					{
						PostBody = string.IsNullOrEmpty(PostBody) ? null : string.Format(PostBody, j),
						Origin = Origin,
						Method = Method,
						Referer = Referer
					});
				}
			}
		}
	}

	public class DateCyclePrepareStartUrls : PrepareStartUrls
	{
		public DateTime From { get; set; }
		public DateTime To { get; set; }

		public int IntervalDay { get; set; } = 1;
		public string DateFormate { get; set; } = "yyyy-MM-dd";
		public string FormateString { get; set; }

		public override void Build(Spider spider, dynamic obj)
		{
			Dictionary<string, object> data = new Dictionary<string, object>();

			if (Extras != null)
			{
				foreach (var extra in Extras)
				{
					data.Add(extra.Key, extra.Value);
				}
			}

			data.Add("DateTime", "");

			for (DateTime i = From; i <= To; i = i.AddDays(IntervalDay))
			{
				var postBody = string.Format(PostBody, i.ToString(DateFormate));
				data["DateTime"] = i.ToString(DateFormate);
				spider.Scheduler.Push(new Request(string.Format(FormateString, i.ToString(DateFormate)), data)
				{
					PostBody = postBody,
					Origin = Origin,
					Method = Method,
					Referer = Referer
				});
			}
		}
	}

	public class BaseDbPrepareStartUrls : PrepareStartUrls
	{
		public DataSource Source { get; set; } = DataSource.MySql;

		public string ConnectString { get; set; }

		public string QueryString { get; set; }

		/// <summary>
		/// 用于拼接Url所需要的列
		/// </summary>
		public DataColumn[] Columns { get; set; }

		/// <summary>
		/// 拼接Url的方式, 会把Columns对应列的数据传入
		/// https://s.taobao.com/search?q={0},s=0;
		/// </summary>
		public List<string> FormateStrings { get; set; }

		protected List<Dictionary<string, object>> PrepareDatas()
		{
			List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
			NetworkCenter.Current.Execute("db-pstu", () =>
			{
				using (var conn = DataSourceUtil.GetConnection(Source, ConnectString))
				{
					conn.Open();
					var command = conn.CreateCommand();
					command.CommandText = QueryString;
					command.CommandTimeout = 60000;
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

						if (Extras != null)
						{
							foreach (var extra in Extras)
							{
								data.Add(extra.Key, extra.Value);
							}
						}
						list.Add(data);
					}
#if !NET_CORE
					reader.Close();
#else
				reader.Dispose();
#endif
				}
			});

			return list;
		}

		protected virtual List<string> PrepareArguments(Dictionary<string, object> data)
		{
			List<string> arguments = new List<string>();
			if (Columns != null)
			{
				foreach (var column in Columns)
				{
					string value = data[column.Name]?.ToString();

					if (column.Formatters != null)
					{
						foreach (var formatter in column.Formatters)
						{
							value = formatter.Formate(value);
						}
					}
					arguments.Add(value);
				}
			}
			return arguments;
		}

		public override void Build(Spider spider, dynamic obj)
		{
			if (string.IsNullOrEmpty(QueryString))
			{
				throw new SpiderException("QueryString is null.");
			}

			var datas = PrepareDatas();
			foreach (var data in datas)
			{
				var arguments = PrepareArguments(data);

				foreach (var formate in FormateStrings)
				{
					string tmpUrl = string.Format(formate, arguments.Cast<object>().ToArray());
					spider.Scheduler.Push(new Request(tmpUrl, data)
					{
						Method = Method,
						Origin = Origin,
						PostBody = GetPostBody(PostBody, data),
						Referer = Referer
					});
				}
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
				int startIndex = tmp.IndexOf("__URLENCODE('", StringComparison.Ordinal);
				int endIndex = tmp.IndexOf("')", startIndex, StringComparison.Ordinal);
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
	}

	public class ListPrepareStartUrls : PrepareStartUrls
	{
		public List<string> Data { get; set; } = new List<string>();
		public string DataKey { get; set; } = "Data";

		public string FormateString { get; set; }

		public override void Build(Spider spider, dynamic obj)
		{
			Dictionary<string, object> data = new Dictionary<string, object>();

			if (Extras != null)
			{
				foreach (var extra in Extras)
				{
					data.Add(extra.Key, extra.Value);
				}
			}

			data.Add(DataKey, "");

			if (Data.Count > 0)
			{
				foreach (var d in Data)
				{
					data[DataKey] = d;
					spider.Scheduler.Push(new Request(string.Format(FormateString, d), data)
					{
						PostBody = string.IsNullOrEmpty(PostBody) ? null : string.Format(PostBody, d),
						Origin = Origin,
						Method = Method,
						Referer = Referer
					});
				}
			}
			else
			{
				spider.Scheduler.Push(new Request(FormateString, data)
				{
					PostBody = string.IsNullOrEmpty(PostBody) ? null : PostBody,
					Origin = Origin,
					Method = Method,
					Referer = Referer
				});
			}
		}
	}

	public class CommonDbPrepareStartUrls : BaseDbPrepareStartUrls
	{
		public int From { get; set; }
		public int To { get; set; }
		public int Interval { get; set; } = 1;

		public int PostInterval { get; set; } = 1;
		public int PostFrom { get; set; }
		public int PostTo { get; set; }

		public string CookieString { get; set; }

		public override void Build(Spider spider, dynamic obj)
		{
			if (string.IsNullOrEmpty(QueryString))
			{
				throw new SpiderException("QueryString is null.");
			}

			var datas = PrepareDatas();
			foreach (var data in datas)
			{
				var arguments = PrepareArguments(data);
				if (!string.IsNullOrEmpty(CookieString))
				{
					spider.Site.Cookies.StringPart = string.Format(CookieString, arguments.Cast<object>().ToArray());
				}
				for (int i = From; i <= To; i += Interval)
				{
					arguments.Add(i.ToString());
					for (int j = PostFrom; j <= PostTo; j += PostInterval)
					{
						arguments.Add(j.ToString());
						foreach (var formate in FormateStrings)
						{
							string tmpUrl = string.Format(formate, arguments.Cast<object>().ToArray());
							if (data.ContainsKey("DotnetSpiderEnvironmentIndex"))
							{
								data["DotnetSpiderEnvironmentIndex"] = i;
							}
							else
							{
								data.Add("DotnetSpiderEnvironmentIndex", i);
							}

							if (data.ContainsKey("DotnetSpiderEnvironmentPostIndex"))
							{
								data["DotnetSpiderEnvironmentPostIndex"] = j;
							}
							else
							{
								data.Add("DotnetSpiderEnvironmentPostIndex", j);
							}

							spider.Scheduler.Push(new Request(tmpUrl, data)
							{
								Method = Method,
								Origin = !string.IsNullOrEmpty(Origin) ? string.Format(Origin, arguments.Cast<object>().ToArray()) : null,
								PostBody = !string.IsNullOrEmpty(PostBody) ? string.Format(PostBody, arguments.Cast<object>().ToArray()) : null,
								Referer = !string.IsNullOrEmpty(Referer) ? string.Format(Referer, arguments.Cast<object>().ToArray()) : null
							});
						}
						arguments.RemoveAt(arguments.Count - 1);
					}
					arguments.RemoveAt(arguments.Count - 1);
				}
			}
		}

		protected string GetPostBody(string postBody, Dictionary<string, object> data, int i)
		{
			if (!string.IsNullOrEmpty(postBody))
			{
				var arguments = PrepareArguments(data);
				arguments.Add(i.ToString());
				string s = string.Format(postBody, arguments.Cast<object>().ToArray());
				return s;
			}
			return null;
		}
	}

	public class SplitDbPrepareStartUrls : CommonDbPrepareStartUrls
	{
		protected string ColumnName { get; set; }
		protected new List<dynamic> PrepareArguments(Dictionary<string, object> data)
		{
			List<dynamic> arguments = new List<dynamic>();
			if (Columns != null)
			{
				foreach (var column in Columns)
				{
					string value = data[column.Name]?.ToString();

					if (column.Formatters != null)
					{
						foreach (var formatter in column.Formatters)
						{
							var tmpValue = formatter.Formate(value);
							if (tmpValue is List<string>)
							{
								if (string.IsNullOrEmpty(ColumnName))
								{
									ColumnName = column.Name;
								}
								else
								{
									if (column.Name != ColumnName)
									{
										throw new SpiderException("SplitDbPrepareStartUrl Only Supports 1 Column to Be Formatted to List For Now!");
									}
								}
							}
							arguments.Add(tmpValue);
						}
					}
				}
			}
			return arguments;
		}

		public override void Build(Spider spider, dynamic obj)
		{
			if (string.IsNullOrEmpty(QueryString))
			{
				throw new SpiderException("QueryString is null.");
			}

			var datas = PrepareDatas();
			foreach (var data in datas)
			{
				for (int i = From; i <= To; i += Interval)
				{
					var arguments = PrepareArguments(data);
					arguments.Add(i.ToString());

					for (int j = PostFrom; j <= PostTo; j += PostInterval)
					{
						foreach (var formate in FormateStrings)
						{
							bool canStop;
							do
							{
								canStop = true;
								List<string> args = new List<string>();
								foreach (var argument in arguments)
								{
									if (!(argument is List<string>))
									{
										args.Add(argument);
									}
									else
									{
										if (argument.Count > 0)
										{
											args.Add(argument[0]);
											argument.RemoveAt(0);
											canStop = false;
										}
									}
								}
								if (!canStop)
								{
									string tmpUrl = string.Format(formate, args.Cast<object>().ToArray());
									spider.Scheduler.Push(new Request(tmpUrl, data)
									{
										Method = Method,
										Origin = Origin,
										PostBody = GetPostBody(PostBody, data, j),
										Referer = Referer
									});
								}
							} while (!canStop);
						}
					}
				}
			}
		}
	}

	public class LinkSpiderPrepareStartUrls : PrepareStartUrls
	{
		/// <summary>
		/// 用于拼接Url所需要的列
		/// </summary>
		public List<DataColumn> Columns { get; set; } = new List<DataColumn>();

		/// <summary>
		/// 拼接Url的方式, 会把Columns对应列的数据传入
		/// https://s.taobao.com/search?q={0},s=0;
		/// </summary>
		public List<string> FormateStrings { get; set; }

		public override void Build(Spider spider, dynamic obj)
		{
			foreach (JObject jobject in obj)
			{
				Dictionary<string, object> tmp = obj;

				if (Extras != null)
				{
					foreach (var extra in Extras)
					{
						tmp.Add(extra.Key, extra.Value);
					}
				}

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
					spider.Scheduler.Push(new Request(tmpUrl, tmp)
					{
						Method = Method,
						Origin = Origin,
						PostBody = BaseDbPrepareStartUrls.GetPostBody(PostBody, tmp),
						Referer = Referer
					});
				}
			}
		}
	}
}

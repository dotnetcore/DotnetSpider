using System.Data;
using System.Text;

namespace DotnetSpider.Common
{
	public static class DbConnectionExtensions
	{
		/// <summary>
		/// Build HTML table from sql query result.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 把SQL查询结果拼装成HTML的table
		/// </summary>
		/// <param name="conn">连接对象 <see cref="IDbConnection"/></param>
		/// <param name="sql">SQL语句 </param>
		/// <returns>HTML的table, HTML table</returns>
		public static string ToHtml(this IDbConnection conn, string sql)
		{
			var command = conn.CreateCommand();
			command.CommandText = sql;
			command.CommandType = CommandType.Text;

			if (conn.State == ConnectionState.Closed)
			{
				conn.Open();
			}

			IDataReader reader = null;
			try
			{
				reader = command.ExecuteReader();

				int row = 1;
				StringBuilder html = new StringBuilder("<table>");
				while (reader.Read())
				{
					if (row == 1)
					{
						html.Append("<tr>");
						for (int i = 1; i < reader.FieldCount + 1; ++i)
						{
							html.Append($"<td>{reader.GetName(i - 1)}</td>");
						}

						html.Append("</tr>");
					}

					html.Append("<tr>");
					for (int j = 1; j < reader.FieldCount + 1; ++j)
					{
						html.Append($"<td>{reader.GetValue(j - 1)}</td>");
					}

					html.Append("</tr>");
					row++;
				}

				html.Append("</table>");

				return html.ToString();
			}
			finally
			{
				reader?.Close();
			}
		}

	}
}
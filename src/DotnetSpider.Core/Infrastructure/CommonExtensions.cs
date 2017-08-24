using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Core.Infrastructure
{
	public static class CommonExtensions
	{
		public static string ToHTML(this IDbConnection conn, string sql)
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

					var realRowIndx = row + 1;
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

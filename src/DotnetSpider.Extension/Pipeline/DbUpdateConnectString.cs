using System.Data;
using DotnetSpider.Extension.Infrastructure;

namespace DotnetSpider.Extension.Pipeline
{
	public class DbUpdateConnectString : IUpdateConnectString
	{
		public string ConnectString { get; set; }
		public DataSource DataSource { get; set; } = DataSource.MySql;
		public string QueryString { get; set; }

		public string GetNew()
		{
			using (var conn = DataSourceUtil.GetConnection(DataSource, ConnectString))
			{
				string connectString = "";
				var cmd = conn.CreateCommand();
				cmd.CommandText = QueryString;
				cmd.CommandType = CommandType.Text;
				conn.Open();
				var reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					connectString = reader.GetString(0);
				}
				conn.Close();
				return connectString;
			}
		}
	}
}

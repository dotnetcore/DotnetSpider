using System.Data;
using DotnetSpider.Extension.Common;

namespace DotnetSpider.Extension.Pipeline
{
	public class DbUpdateConnectString : IUpdateConnectString
	{
		public string ConnectString { get; set; }
		public DataSource DataSource { get; set; } = DataSource.MySql;
		public string Key { get; set; }

		public string GetNew()
		{
			using (var conn = DataSourceUtil.GetConnection(DataSource, ConnectString))
			{
				string connectString = "";
				var cmd = conn.CreateCommand();
				cmd.CommandText = $"SELECT value from `dotnetspider`.`settings` where `type`='ConnectString' and `key`='{Key}' LIMIT 1";
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

using Dapper;
using System.Linq;

namespace DotnetSpider.Extension.Infrastructure
{
	public interface IUpdateConnectString
	{
		string GetNew();
	}

	public class DbUpdateConnectString : IUpdateConnectString
	{
		public string ConnectString { get; set; }

		public DataSource DataSource { get; set; } = DataSource.MySql;

		public string QueryString { get; set; }

		public string GetNew()
		{
			using (var conn = DataSourceUtils.GetConnection(DataSource, ConnectString))
			{
				string connectString = conn.Query<string>(QueryString).FirstOrDefault();
				return connectString;
			}
		}
	}
}

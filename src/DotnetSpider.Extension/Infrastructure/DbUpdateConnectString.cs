using DotnetSpider.Core.Infrastructure.Database;
using System.Configuration;
using System.Linq;

namespace DotnetSpider.Extension.Infrastructure
{
	public interface IUpdateConnectString
	{
		ConnectionStringSettings GetNew();
	}

	public class DbUpdateConnectString : IUpdateConnectString
	{
		public string ConnectString { get; set; }

		public Database DataSource { get; set; } = Database.MySql;

		public string QueryString { get; set; }

		public ConnectionStringSettings GetNew()
		{
			using (var conn = DatabaseExtensions.GetDbConnection(DataSource, ConnectString))
			{
				ConnectionStringSettings connectString = conn.MyQuery<ConnectionStringSettings>(QueryString).FirstOrDefault();
				return connectString;
			}
		}

	
	}
}

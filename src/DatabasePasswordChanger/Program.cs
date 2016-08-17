using System;
using System.IO;
using System.Text.RegularExpressions;
using Dapper;
using System.Threading;
using MySql.Data.MySqlClient;
using NLog;
using NLog.Config;
using System.Data.SqlClient;
#if NET_CORE
using System.Text;
using Microsoft.Extensions.Configuration;
#endif

namespace DatabasePasswordChanger
{
	public class Program
	{
		private static ILogger _logger;

		public static void Main(string[] args)
		{
#if NET_CORE
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			var nlogConfigPath = Path.Combine(AppContext.BaseDirectory, "nlog.config");
#else
			var nlogConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "nlog.config");
#endif
			LogManager.Configuration = new XmlLoggingConfiguration(nlogConfigPath);
			_logger = LogManager.GetCurrentClassLogger();

			_logger.Info("STEP 1: Start change password.");

			try
			{
				string connectString = Configuration.GetValue("mysql");
				string targetConnectString = Configuration.GetValue("targetMysql");
				using (MySqlConnection conn = new MySqlConnection(targetConnectString))
				{
					_logger.Info("STEP 2: Add new mysql account.");

					string date = DateTime.Now.ToString("yyyyMMddHHmmss");
					string password = Guid.NewGuid().ToString("N");
					string user = $"autouser{date}";
					conn.Execute($"CREATE USER IF NOT EXISTS {user}@'%' IDENTIFIED BY '{password}';");
					conn.Execute($"GRANT ALL ON *.* TO '{user}'@'%';");
					conn.Execute("FLUSH PRIVILEGES;");

					_logger.Info("STEP 3: Delete old mysql account.");

					conn.Execute($"DELETE FROM mysql.user where User like 'autouser%' and User!='{user}';");

					using (MySqlConnection conn1 = new MySqlConnection(connectString))
					{
						Regex portRegex = new Regex("Port=[0-9]+");
						var match = portRegex.Match(targetConnectString);
						string newConnectString =
							$"Database='mysql';Data Source={conn.DataSource};User ID={user};Password={password};{match.Value}";

						bool testNewConnectString = true;
						try
						{
							using (MySqlConnection conn2 = new MySqlConnection(newConnectString))
							{
								conn2.Open();
								conn2.Query("SELECT * FROM mysql.user;");
								_logger.Info("STEP 4: Test mysql new account sucess.");
							}
						}
						catch
						{
							testNewConnectString = false;
							_logger.Info("STEP 4: Test mysql new account failed.");
						}

						if (testNewConnectString)
						{
							conn1.Execute(Configuration.GetValue("createDatabase"));
							conn1.Execute(Configuration.GetValue("creatTable"));
							var enumerator = conn1.Query(Configuration.GetValue("checkSql")).GetEnumerator();
							enumerator.MoveNext();
							if (enumerator.Current.Count > 0)
							{
								conn1.Execute(string.Format(Configuration.GetValue("updateSql").Replace("\\\"", "\""), newConnectString));
								_logger.Info("STEP 5: Update settings sucess.");
							}
							else
							{
								conn1.Execute(string.Format(Configuration.GetValue("insertSql").Replace("\\\"", "\""), newConnectString));
								_logger.Info("STEP 5: Add settings sucess.");
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				_logger.Error(e, "Change password failed.");
			}

			for (int i = 0; i < 6; ++i)
			{
				Thread.Sleep(1000);
			}
		}
	}
}

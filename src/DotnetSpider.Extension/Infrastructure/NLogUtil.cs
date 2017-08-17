using MySql.Data.MySqlClient;
using System.IO;
using System.Linq;
using Dapper;
using System.Xml.Linq;

namespace DotnetSpider.Extension.Infrastructure
{
	public class NLogUtil
	{
		public static void PrepareDatabase(string connectString)
		{
			var fileInfo = new FileInfo(Path.Combine(Core.Infrastructure.Environment.BaseDirectory, "nlog.config"));
			if (fileInfo.Exists)
			{
				XElement root = XElement.Parse(File.ReadAllText(fileInfo.FullName));

				var targetsRoot = root.Element("{http://www.nlog-project.org/schemas/NLog.xsd}targets");
				var targets = targetsRoot.Elements("{http://www.nlog-project.org/schemas/NLog.xsd}target").ToList();
				var dblog = targets.First(e => e.Attribute("name").Value == "dblog");
				var commands = dblog.Elements("{http://www.nlog-project.org/schemas/NLog.xsd}install-command");
				using (var conn = new MySqlConnection(connectString))
				{
					foreach (var command in commands)
					{
						var sql = command.Attribute("text").Value;
						try
						{
							conn.Execute(sql);
						}
						catch
						{
						}
					}
				}
			}
		}
	}
}

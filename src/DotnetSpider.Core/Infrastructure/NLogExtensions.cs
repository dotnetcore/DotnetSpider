using System.IO;
using System.Linq;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace DotnetSpider.Core.Infrastructure
{
	public static class NLogExtensions
	{
		private static string _init = "F";

		public static void Init()
		{
			lock (_init)
			{
				if (_init != "S")
				{
					string nlogConfigPath = Path.Combine(SpiderConsts.BaseDirectory, "nlog.config");

					if (!File.Exists(nlogConfigPath))
					{
						File.AppendAllText(nlogConfigPath, NlogDefaultConfig);
					}
					XmlLoggingConfiguration configuration = new XmlLoggingConfiguration(nlogConfigPath);
					var connectString = Configuration.GetValue("logAndStatusConnectString");
					var logAndStatusTargets = configuration.AllTargets.Where(t => t.Name == "dblog" || t.Name == "dbstatus").ToList();
					if (!string.IsNullOrEmpty(connectString))
					{
						foreach (var logAndStatusTarget in logAndStatusTargets)
						{
							DatabaseTarget dbTarget = (DatabaseTarget)logAndStatusTarget;
							dbTarget.ConnectionString = connectString;
						}
					}
					else
					{
						var needDeleteRules = configuration.LoggingRules.Where(r => r.Targets.Any(t => t is DatabaseTarget && ((DatabaseTarget)t).ConnectionString == null)).ToList();
						foreach (var rule in needDeleteRules)
						{
							configuration.LoggingRules.Remove(rule);
						}
						configuration.RemoveTarget("dblog");
						configuration.RemoveTarget("dbstatus");
					}

					configuration.Install(new InstallationContext());
					LogManager.Configuration = configuration;
					_init = "S";
				}
			}
		}

		private const string NlogDefaultConfig = "<nlog xmlns=\"http://www.nlog-project.org/schemas/NLog.xsd\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" throwExceptions=\"true\"><targets><target name = \"console\" xsi:type=\"ColoredConsole\" useDefaultRowHighlightingRules=\"true\" layout=\"[${date:format=yyyyMMdd HH\\:mm\\:ss}][${event-properties:item=UserId}][${event-properties:item=TaskGroup}][${level}] ${message}\"><highlight-row foregroundColor = \"Cyan\" condition=\"level == LogLevel.Trace\"/><highlight-row foregroundColor = \"DarkGray\" condition=\"level == LogLevel.Debug\"/></target><target name = \"file\" xsi:type=\"File\" maxArchiveFiles=\"30\" layout=\"[${date:format=yyyyMMdd HH\\:mm\\:ss}][${event-properties:item=UserId}][${event-properties:item=TaskGroup}][${level}] ${message}\" fileName=\"${basedir}/logs/log${shortdate}.txt\" keepFileOpen=\"false\" /></targets><rules><logger name = \"*\" minlevel=\"Trace\" writeTo=\"console\" /><logger name = \"*\" minlevel=\"Trace\" writeTo=\"file\" /></rules></nlog>";
	}
}

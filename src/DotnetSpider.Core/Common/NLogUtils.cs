using System.IO;
using System.Linq;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace DotnetSpider.Core.Common
{
	public class NLogUtils
	{
		private static bool _init;

		public static void Init(bool forceInit = false)
		{
			if (!_init || forceInit)
			{
#if NET_CORE
				string nlogConfigPath = Path.Combine(SpiderConsts.BaseDirectory, "nlog.netcore.config");
#else
				string nlogConfigPath = Path.Combine(SpiderConsts.BaseDirectory, "nlog.net45.config");
#endif
				if (!File.Exists(nlogConfigPath))
				{
					File.AppendAllText(nlogConfigPath, Resource.nlog);
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
				_init = true;
			}
		}
	}
}

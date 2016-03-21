using System;
using System.IO;
#if NET_CORE
using System.Runtime.InteropServices;
#endif

namespace Java2Dotnet.Spider.Redial.Utils
{
	public static class AdslUtil
	{
		public static void Connect(string connectionName, string user, string pass)
		{
			Disconnect(connectionName);
			string arg;
#if !NET_CORE
			arg = $"rasdial \"{connectionName}\" {user} {pass}";
#else
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				arg = "adsl-start";
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				arg = Path.Combine("~/dotnetspider");
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				arg = $"rasdial \"{connectionName}\" {user} {pass}";
			}
			else
			{
				throw new ArgumentException("Unknow OS.");
			}
#endif


			CmdUtil.InvokeCmd(arg);
		}

		private static void Disconnect(string connectionName)
		{
			string arg = $"rasdial \"{connectionName}\" /disconnect";
			CmdUtil.InvokeCmd(arg);
		}
	}
}

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using DotnetSpider.Common;
using DotnetSpider.DownloadAgent;

namespace DotnetSpider.Network
{
	/// <summary>
	/// ADSL 拨号器
	/// </summary>
	public class DefaultAdslRedialer : AdslRedialerBase
	{
		/// <summary>
		/// 构造方法
		/// </summary>
		public DefaultAdslRedialer(DownloaderAgentOptions options) : base(options)
		{
		}

		/// <summary>
		/// 拨号
		/// </summary>
		public override bool Redial()
		{
#if NETFRAMEWORK
			return RedialOnWindows();
#else
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				KillPppoeProcesses();
				Process process = Process.Start("/sbin/ifdown", "ppp0");
				if (process == null)
				{
					return false;
				}

				process.WaitForExit();
				process = Process.Start("/sbin/ifup", "ppp0");
				if (process == null)
				{
					return false;
				}

				process.WaitForExit();
				return true;
			}

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				return RedialOnWindows();
			}

			throw new PlatformNotSupportedException($"{Environment.OSVersion.Platform}");
#endif
		}

		private bool RedialOnWindows()
		{
			Rasdial adsl =
				new Rasdial(Options.AdslInterface, Options.AdslAccount, Options.AdslPassword);
			adsl.Disconnect();
			while (adsl.Connect() != 0)
			{
				Thread.Sleep(1000);
			}

			return true;
		}

#if NETSTANDARD || NETCOREAPP
		private void KillPppoeProcesses()
		{
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				var processes = Process.GetProcessesByName("pppd").ToList();
				processes.AddRange(Process.GetProcessesByName("pppoe"));
				foreach (var process in processes)
				{
					try
					{
						process.Kill();
					}
					catch
					{
						// ignore
					}
				}
			}
		}
#endif
	}
}
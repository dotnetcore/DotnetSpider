using System;
using System.IO;
using System.Threading;
#if NETSTANDARD || NETCOREAPP
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
#endif

namespace DotnetSpider.Downloader.Redial.Redialer
{
	/// <summary>
	/// ADSL拨号器
	/// </summary>
	public class DefaultAdslRedialer : AdslRedialer
	{
		/// <summary>
		/// 构造方法
		/// </summary>
		public DefaultAdslRedialer() : this(Path.Combine(DownloaderEnv.GlobalDirectory, "adsl_account.txt"))
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="configPath">配置文件路径. 配置文件内容需要三行, 按顺序为: 网络接口名称, 帐号, 密码</param>
		public DefaultAdslRedialer(string configPath)
		{
			if (File.Exists(configPath))
			{
				var accounts = File.ReadAllLines(configPath);
				Interface = accounts[0].Trim();
				Account = accounts[1].Trim();
				Password = accounts[2].Trim();
			}
			else
			{
				throw new ArgumentException($"Unfound adsl config: {configPath}");
			}
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="interfaceName">网络接口名称</param>
		/// <param name="user">帐号</param>
		/// <param name="password">密码</param>
		public DefaultAdslRedialer(string interfaceName, string user, string password)
		{
			Interface = interfaceName;
			Account = user;
			Password = password;
		}

		/// <summary>
		/// 拨号
		/// </summary>
		public override void Redial()
		{
#if NETFRAMEWORK
			RedialOnWindows();
#else
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				KillPPPOEProcesses();
				Process process = Process.Start("/sbin/ifdown", "ppp0");
				if (process == null)
				{
					throw new  RedialException("Disconnect adsl failed.");
				}
				process.WaitForExit();
				process = Process.Start("/sbin/ifup", "ppp0");
				if (process == null)
				{
					throw new  RedialException("Connect adsl failed.");
				}
				process.WaitForExit();
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				RedialOnWindows();
			}
			else
			{
				throw new PlatformNotSupportedException($"{Environment.OSVersion.Platform}");
			}
#endif
		}

		private void RedialOnWindows()
		{
			Rasdial adsl = new Rasdial(Interface, Account, Password);
			adsl.Disconnect();
			while (adsl.Connect() != 0)
			{
				Thread.Sleep(2000);
			}
		}
#if NETSTANDARD || NETCOREAPP
		private void KillPPPOEProcesses()
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

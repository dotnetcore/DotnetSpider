using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotnetSpider.Http;
using Microsoft.Extensions.Options;

namespace DotnetSpider.Downloader
{
	public class PPPoEService
	{
		private readonly PPPoEOptions _options;

		public PPPoEService(IOptions<PPPoEOptions> options)
		{
			_options = options.Value;
		}

		public bool IsActive => !string.IsNullOrWhiteSpace(_options.Account) &&
		                        !string.IsNullOrWhiteSpace(_options.Password) &&
		                        !string.IsNullOrWhiteSpace(_options.Interface);

		/// <summary>
		/// 异步拨号，直接先返回结果，爬虫会重试发到别的代理器上
		/// 拨号也不需要等待其它下载完成，除非先下线节点，再等待所有下载完成
		/// 再拨号，拨号成功后再重新订阅，怎么个逻辑太复杂。
		/// ADSL 本身就不可能非常快，因此直接拨号触发重试即可，只要节点够多，完全可以接受
		/// </summary>
		/// <param name="request"></param>
		/// <param name="response"></param>
		/// <returns></returns>
		public Task<string> DetectAsync(Request request, string response)
		{
			var pattern = request.PPPoERegex;
			if (IsActive && !string.IsNullOrWhiteSpace(pattern))
			{
				var match = Regex.Match(response, pattern);
				if (match.Success)
				{
					Redial();
					return Task.FromResult(match.Value);
				}
			}

			return null;
		}

		private void Redial()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				KillPPPoEProcesses();
				var process = Process.Start("/sbin/ifdown", "ppp0");
				if (process == null)
				{
					return;
				}

				process.WaitForExit();
				process = Process.Start("/sbin/ifup", "ppp0");
				if (process == null)
				{
					return;
				}

				process.WaitForExit();
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				RedialOnWindows();
				return;
			}

			throw new PlatformNotSupportedException($"{Environment.OSVersion.Platform}");
		}

		private void RedialOnWindows()
		{
			var process = new Process
			{
				StartInfo =
				{
					FileName = "rasdial.exe",
					UseShellExecute = false,
					CreateNoWindow = false,
					WorkingDirectory = @"C:\Windows\System32",
					Arguments = _options.Interface + @" /DISCONNECT"
				}
			};
			process.Start();
			process.WaitForExit(10000);

			process = new Process
			{
				StartInfo =
				{
					FileName = "rasdial.exe",
					UseShellExecute = false,
					CreateNoWindow = false,
					WorkingDirectory = @"C:\Windows\System32",
					Arguments = _options.Interface + " " + _options.Account + " " +
					            _options.Password
				}
			};
			process.Start();
			process.WaitForExit(10000);
		}

		private void KillPPPoEProcesses()
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
	}
}

using System.Diagnostics;

namespace DotnetSpider.Core.Infrastructure
{
	public class AdslCommand
	{
		/// <summary>
		/// 拨号名称
		/// </summary>
		public string InterfaceName { get; }

		public string Username { get; }
		/// <summary>
		/// 密码
		/// </summary>
		public string Password { get; }

		public AdslCommand(string interfaceName, string username = null, string password = null)
		{
			InterfaceName = interfaceName;
			Username = username;
			Password = password;
		}

		/// <summary>
		/// 开始拨号
		/// </summary>
		/// <returns>返回拨号进程的返回值</returns>
		public int Connect()
		{
			Process process = new Process
			{
				StartInfo =
					{
						FileName = "rasdial.exe",
						UseShellExecute = false,
						CreateNoWindow = false,
						WorkingDirectory = @"C:\Windows\System32",
						Arguments = InterfaceName + " " + Username + " " + Password
					}
			};
			process.Start();
			process.WaitForExit(10000);
			return process.ExitCode;
		}

		/// <summary>
		/// 端口连接
		/// </summary>
		/// <returns></returns>
		public int Disconnect()
		{
			Process process = new Process
			{
				StartInfo =
					{
						FileName = "rasdial.exe",
						UseShellExecute = false,
						CreateNoWindow = false,
						WorkingDirectory = @"C:\Windows\System32",
						Arguments = InterfaceName + @" /DISCONNECT"
					}
			};
			process.Start();
			process.WaitForExit(10000);
			return process.ExitCode;
		}
	}
}

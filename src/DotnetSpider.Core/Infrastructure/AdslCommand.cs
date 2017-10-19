using System.Diagnostics;

namespace DotnetSpider.Core.Infrastructure
{
	public class AdslCommand
	{
		/// <summary>
		/// 拨号名称
		/// </summary>
		private readonly string _interfaceName;

		private readonly  string _username;

		/// <summary>
		/// 密码
		/// </summary>
		private readonly  string _password;

		public AdslCommand(string interfaceName, string username = null, string password = null)
		{
			_interfaceName = interfaceName;
			_username = username;
			_password = password;
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
						Arguments = _interfaceName + " " + _username + " " + _password
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
		public void Disconnect()
		{
			Process process = new Process
			{
				StartInfo =
					{
						FileName = "rasdial.exe",
						UseShellExecute = false,
						CreateNoWindow = false,
						WorkingDirectory = @"C:\Windows\System32",
						Arguments = _interfaceName + @" /DISCONNECT"
					}
			};
			process.Start();
			process.WaitForExit(10000);
		}
	}
}

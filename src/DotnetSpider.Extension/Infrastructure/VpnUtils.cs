#if !NET_CORE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using DotRas;
using System.Text.RegularExpressions;
using System.Threading;

namespace DotnetSpider.Extension.Infrastructure
{
	public class VpnUtils
	{
		// 系统路径 C:\windows\system32\
		private static readonly string WinDir = Environment.GetFolderPath(Environment.SpecialFolder.System) + @"\";
		// rasdial.exe
		private const string RasDialFileName = "rasdial.exe";
		// Vpn路径 C:\windows\system32\rasdial.exe
		private static readonly string Vpnprocess = WinDir + RasDialFileName;
		// Vpn地址
		public string IpToPing { get; set; }
		// Vpn名称
		public string VpnName { get; set; }
		// Vpn用户名
		public string UserName { get; set; }
		// Vpn密码
		public string PassWord { get; set; }

		public VpnUtils()
		{
		}
		/// <summary>
		/// 带参构造函数
		/// </summary>
		/// <param name="vpnIp"></param>
		/// <param name="vpnName"></param>
		/// <param name="userName"></param>
		/// <param name="passWord"></param>
		public VpnUtils(string vpnIp, string vpnName, string userName, string passWord)
		{
			IpToPing = vpnIp;
			VpnName = vpnName;
			UserName = userName;
			PassWord = passWord;
		}
		/// <summary>
		/// 尝试连接Vpn(默认Vpn)
		/// </summary>
		/// <returns></returns>
		public void TryConnectVpn()
		{
			TryConnectVpn(VpnName, UserName, PassWord);
		}
		/// <summary>
		/// 尝试断开连接(默认Vpn)
		/// </summary>
		/// <returns></returns>
		public void TryDisConnectVpn()
		{
			TryDisConnectVpn(VpnName);
		}
		/// <summary>
		/// 创建或更新一个默认的Vpn连接
		/// </summary>
		public void CreateOrUpdateVpn()
		{
			CreateOrUpdateVpn(VpnName, IpToPing);
		}
		/// <summary>
		/// 尝试删除连接(默认Vpn)
		/// </summary>
		/// <returns></returns>
		public void TryDeleteVpn()
		{
			TryDeleteVpn(VpnName);
		}
		/// <summary>
		/// 尝试连接Vpn(指定Vpn名称，用户名，密码)
		/// </summary>
		/// <returns></returns>
		public void TryConnectVpn(string connVpnName, string connUserName, string connPassWord)
		{
			try
			{
				string args = $"{connVpnName} {connUserName} {connPassWord}";
				ProcessStartInfo myProcess = new ProcessStartInfo(Vpnprocess, args)
				{
					CreateNoWindow = true,
					UseShellExecute = false
				};
				var result = Process.Start(myProcess);
				if (result != null)
				{
					Console.WriteLine("Wait for process to exit.....");
					while (!result.HasExited)
					{
						Thread.Sleep(100);
					}
					result.Close();
					Console.WriteLine("Process closed.....");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				//Debug.Assert(false, ex.ToString());
			}
		}
		/// <summary>
		/// 尝试断开Vpn(指定Vpn名称)
		/// </summary>
		/// <returns></returns>
		public void TryDisConnectVpn(string disConnVpnName)
		{
			try
			{
				string args = $@"{disConnVpnName} /d";
				ProcessStartInfo myProcess = new ProcessStartInfo(Vpnprocess, args)
				{
					CreateNoWindow = true,
					UseShellExecute = false
				};
				var result = Process.Start(myProcess);
				if (result != null)
				{
					Console.WriteLine("Wait for process to exit.....");
					while (!result.HasExited)
					{
						Thread.Sleep(100);
					}
					result.Close();
					Console.WriteLine("Process closed.....");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				//Debug.Assert(false, ex.ToString());
			}
		}
		/// <summary>
		/// 创建或更新一个Vpn连接(指定Vpn名称，及IP)
		/// </summary>
		public void CreateOrUpdateVpn(string updateVpNname, string updateVpNip)
		{
			RasDialer dialer = new RasDialer();
			RasPhoneBook allUsersPhoneBook = new RasPhoneBook();
			allUsersPhoneBook.Open(true);
			// 如果已经该名称的Vpn已经存在，则更新这个Vpn服务器地址
			if (allUsersPhoneBook.Entries.Contains(updateVpNname))
			{
				allUsersPhoneBook.Entries[updateVpNname].PhoneNumber = updateVpNip;
				// 不管当前Vpn是否连接，服务器地址的更新总能成功，如果正在连接，则需要Vpn重启后才能起作用
				allUsersPhoneBook.Entries[updateVpNname].Update();
			}
			// 创建一个新Vpn
			else
			{
				RasEntry entry = RasEntry.CreateVpnEntry(updateVpNname, updateVpNip, RasVpnStrategy.PptpFirst, RasDevice.GetDeviceByName("(PPTP)", RasDeviceType.Vpn));
				allUsersPhoneBook.Entries.Add(entry);
				dialer.EntryName = updateVpNname;
				dialer.PhoneBookPath = RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.AllUsers);
			}
		}
		/// <summary>
		/// 删除指定名称的Vpn
		/// 如果Vpn正在运行，一样会在电话本里删除，但是不会断开连接，所以，最好是先断开连接，再进行删除操作
		/// </summary>
		/// <param name="delVpnName"></param>
		public void TryDeleteVpn(string delVpnName)
		{
			RasPhoneBook allUsersPhoneBook = new RasPhoneBook();
			allUsersPhoneBook.Open();
			if (allUsersPhoneBook.Entries.Contains(delVpnName))
			{
				allUsersPhoneBook.Entries.Remove(delVpnName);
			}
		}
		/// <summary>
		/// 获取当前正在连接中的Vpn名称
		/// </summary>
		public List<string> GetCurrentConnectingVpnNames()
		{
			List<string> connectingVpnList = new List<string>();
			Process proIp = new Process();
			proIp.StartInfo.FileName = "cmd.exe ";
			proIp.StartInfo.UseShellExecute = false;
			proIp.StartInfo.RedirectStandardInput = true;
			proIp.StartInfo.RedirectStandardOutput = true;
			proIp.StartInfo.RedirectStandardError = true;
			proIp.StartInfo.CreateNoWindow = true;//不显示cmd窗口
			proIp.Start();
			proIp.StandardInput.WriteLine(RasDialFileName);
			proIp.StandardInput.WriteLine("exit");
			// 命令行运行结果
			string strResult = proIp.StandardOutput.ReadToEnd();
			proIp.Close();
			// 用正则表达式匹配命令行结果，只限于简体中文系统哦^_^
			Regex regger = new Regex("(?<=已连接\n)(.*\n)*(?=命令已完成)", RegexOptions.Multiline);
			// 如果匹配，则说有正在连接的Vpn
			if (regger.IsMatch(strResult))
			{
				string[] list = regger.Match(strResult).Value.Split('\n');
				for (int index = 0; index < list.Length; index++)
				{
					if (list[index] != string.Empty)
						connectingVpnList.Add(list[index].Replace("\r", ""));
				}
			}
			// 没有正在连接的Vpn，则直接返回一个空List<string>
			return connectingVpnList;
		}
	}
}
#endif
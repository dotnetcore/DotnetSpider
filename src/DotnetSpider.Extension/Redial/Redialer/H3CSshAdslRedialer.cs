using System;
using System.Text;
using System.Threading;
using Renci.SshNet;
using DotnetSpider.Core.Redial.Redialer;

namespace DotnetSpider.Extension.Redial.Redialer
{
	/// <summary>
	/// 针对H3C路由器实现的拨号器
	/// </summary>
	public class H3CSshAdslRedialer : BaseAdslRedialer
	{
		private readonly string _sshhost;
		private readonly string _sshuser;
		private readonly string _sshpass;
		private readonly int _sshport;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="sshhost">路由器网关的IP</param>
		/// <param name="sshPort">路由器SSH的连接端口号, 默认是22</param>
		/// <param name="sshuser">路由器SSH的用户名</param>
		/// <param name="sshpass">路由器SSH的密码</param>
		/// <param name="interfaceName">连接的名称(H3C有些路由器是可以多路ADSL拨号的, 在此指定需要重拨的是哪一路)</param>
		/// <param name="user">ADSL的用户名</param>
		/// <param name="password">ADSL的密码</param>
		public H3CSshAdslRedialer(string sshhost, int sshPort, string sshuser, string sshpass, string interfaceName, string user, string password) : base(interfaceName, user, password)
		{
			_sshhost = sshhost;
			_sshuser = sshuser;
			_sshpass = sshpass;
			_sshport = sshPort;
		}

		/// <summary>
		/// 拨号
		/// </summary>
		public override void Redial()
		{
			using (SshClient client = new SshClient(_sshhost, _sshport, _sshuser, _sshpass))
			{
				client.Connect();

				var shell = client.CreateShellStream("redialer", 120, 120, 120, 120, 1024);
				shell.DataReceived += Shell_DataReceived;

				shell.WriteLine("system-view");
				Thread.Sleep(1000);
				shell.WriteLine("interface " + Interface);
				Thread.Sleep(1000);
				shell.WriteLine("shutdown");
				Thread.Sleep(35000);
				shell.WriteLine("undo shutdown");
				shell.WriteLine("");
				client.Disconnect();
			}
		}

		private void Shell_DataReceived(object sender, Renci.SshNet.Common.ShellDataEventArgs e)
		{
			Console.Write(Encoding.UTF8.GetString(e.Data));
		}
	}
}
#if !NET_CORE

using System;
using System.Text;
using System.Threading;
using Renci.SshNet;
using DotnetSpider.Core.Redial.Redialer;

namespace DotnetSpider.Extension.Redial.Redialer
{
	public class H3CSshAdslRedialer : BaseAdslRedialer
	{
		private readonly string _sshhost;
		private readonly string _sshuser;
		private readonly string _sshpass;
		private readonly int _sshport;

		public H3CSshAdslRedialer(string sshhost,int sshPort, string sshuser, string sshpass, string interfaceName, string user, string password) : base(interfaceName, user, password)
		{
			_sshhost = sshhost;
			_sshuser = sshuser;
			_sshpass = sshpass;
			_sshport = sshPort;
		}

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

#endif
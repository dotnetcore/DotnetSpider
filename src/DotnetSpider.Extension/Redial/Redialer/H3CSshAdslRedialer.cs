#if !NET_CORE

using System;
using System.Configuration;
using System.Text;
using System.Threading;
using Renci.SshNet;

namespace DotnetSpider.Extension.Redial.Redialer
{
	public class H3CSshAdslRedialer : BaseAdslRedialer
	{
		private readonly string _sshhost;
		private readonly string _sshuser;
		private readonly string _sshpass;
		private readonly string _sshInterface;
		private readonly int _sshport;

		public H3CSshAdslRedialer()
		{
			_sshhost = ConfigurationManager.AppSettings["sshHost"];
			_sshuser = ConfigurationManager.AppSettings["sshUser"];
			_sshpass = ConfigurationManager.AppSettings["sshPassword"];
			_sshInterface = ConfigurationManager.AppSettings["sshInterface"];
			var port = ConfigurationManager.AppSettings["sshPort"];
			_sshport = port == null ? 22 : int.Parse(port);
		}

		public H3CSshAdslRedialer(string sshhost,int sshPort, string sshuser, string sshpass, string interface1, string user, string password) : base(interface1, user, password)
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
				shell.WriteLine("interface " + _sshInterface);
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
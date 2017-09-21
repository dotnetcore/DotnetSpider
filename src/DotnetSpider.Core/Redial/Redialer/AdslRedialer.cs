using DotnetSpider.Core.Infrastructure;
using System.IO;
using System.Threading;
#if NET_CORE
using System.Diagnostics;
using System.Runtime.InteropServices;
#endif

namespace DotnetSpider.Core.Redial.Redialer
{
	public class AdslRedialer : BaseAdslRedialer
	{
		public AdslRedialer(string interfaceName, string user, string password) : base(interfaceName, user, password)
		{
		}

		public AdslRedialer() : base("", "", "")
		{
			var path = Path.Combine(Env.GlobalDirectory, "adsl_account.txt");
			var accounts = File.ReadAllLines(path);
			Interface = accounts[0].Trim();
			Account = accounts[1].Trim();
			Password = accounts[2].Trim();
		}

		public override void Redial()
		{
#if !NET_CORE
			AdslCommand adsl = new AdslCommand(Interface, Account, Password);
			adsl.Disconnect();
			while (adsl.Connect() != 0)
			{
				Thread.Sleep(2000);
			}
#else
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				Process process = Process.Start("/sbin/ifdown", "ppp0");
				process.WaitForExit();
				process = Process.Start("/sbin/ifup", "ppp0");
				process.WaitForExit();
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				AdslCommand adsl = new AdslCommand(Interface, Account, Password);
				adsl.Disconnect();
				while (adsl.Connect() != 0)
				{
					Thread.Sleep(2000);
				}
			}
			else
			{
				throw new RedialException("Unsport this OS.");
			}
#endif
		}
	}
}


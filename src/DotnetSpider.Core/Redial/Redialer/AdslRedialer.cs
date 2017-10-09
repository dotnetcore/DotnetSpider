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
		public AdslRedialer() : this(Path.Combine(Env.GlobalDirectory, "adsl_account.txt"))
		{
		}

		public AdslRedialer(string configPath)
		{
			var path = Path.Combine(configPath);
			if (File.Exists(path))
			{
				var accounts = File.ReadAllLines(path);
				Interface = accounts[0].Trim();
				Account = accounts[1].Trim();
				Password = accounts[2].Trim();
			}
			else
			{
				throw new SpiderException($"Unfound adsl config: {path}.");
			}
		}

		public AdslRedialer(string interfaceName, string user, string password)
		{
			Interface = interfaceName;
			Account = user;
			Password = password;
		}

		public override void Redial()
		{

#if !NET_CORE
			RedialOnWindows();
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
				RedialOnWindows();
			}
			else
			{
				throw new RedialException("Unsport this OS.");
			}
#endif
		}

		private void RedialOnWindows()
		{
			AdslCommand adsl = new AdslCommand(Interface, Account, Password);
			adsl.Disconnect();
			while (adsl.Connect() != 0)
			{
				Thread.Sleep(2000);
			}
		}
	}
}


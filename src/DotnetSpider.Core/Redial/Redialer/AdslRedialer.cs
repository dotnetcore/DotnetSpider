
using System.Diagnostics;
using System.Runtime.InteropServices;

using DotnetSpider.Core.Infrastructure;
using System.Threading;

namespace DotnetSpider.Core.Redial.Redialer
{
	public class AdslRedialer : BaseAdslRedialer
	{
		public AdslRedialer(string interfaceName, string user, string password) : base(interfaceName, user, password)
		{
		}

		public override void Redial()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				Process process = Process.Start("adsl-stop");
				process.WaitForExit();
				process = Process.Start("adsl-start");
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
		}
	}
}


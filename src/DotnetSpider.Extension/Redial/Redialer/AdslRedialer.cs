#if NET_CORE
using System.Diagnostics;
using System.Runtime.InteropServices;
#else
using DotnetSpider.Extension.Infrastructure;
#endif

namespace DotnetSpider.Extension.Redial.Redialer
{
	public class AdslRedialer : BaseAdslRedialer
	{
		public AdslRedialer()
		{
		}

		public AdslRedialer(string interface1, string user, string password) : base(interface1, user, password)
		{
		}

		public override void Redial()
		{

#if !NET_CORE
			Adsl ras = new Adsl();
			ras.Disconnect();//断开连接
			ras.Connect(Interface);//重新拨号
#else
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				Process process = Process.Start("adsl-stop");
				process.WaitForExit();
				process = Process.Start("adsl-start");
				process.WaitForExit();
			}
#endif
		}
	}
}


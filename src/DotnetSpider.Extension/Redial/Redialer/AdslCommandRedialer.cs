
#if !NET_CORE
using System.Threading;
using DotnetSpider.Extension.Infrastructure;

namespace DotnetSpider.Extension.Redial.Redialer
{
	public class AdslCommandRedialer : BaseAdslRedialer
	{
		public AdslCommandRedialer()
		{
		}

		public AdslCommandRedialer(string interface1, string user, string password) : base(interface1, user, password)
		{
		}

		public override void Redial()
		{
			AdslCommandUtil adsl = new AdslCommandUtil(Interface, Account, Password);
			adsl.Disconnect();
			while (adsl.Connect() != 0)
			{
				Thread.Sleep(2000);
			}
		}
	}
}
#endif

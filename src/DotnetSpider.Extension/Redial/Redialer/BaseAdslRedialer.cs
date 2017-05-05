#if !NET_CORE
using System.Configuration;
#endif

namespace DotnetSpider.Extension.Redial.Redialer
{
	public abstract class BaseAdslRedialer : IRedialer
	{
		public string Interface { get; set; }
		public string Account { get; set; }
		public string Password { get; set; }

		protected BaseAdslRedialer()
		{
#if !NET_CORE
			var interface1 = ConfigurationManager.AppSettings["redialInterface"];
			Interface = string.IsNullOrEmpty(interface1) ? "宽带连接" : interface1;

			Account = ConfigurationManager.AppSettings["redialUser"];
			Password = ConfigurationManager.AppSettings["redialPassword"];
#endif
		}

		protected BaseAdslRedialer(string interface1, string account, string password)
		{
			Interface = interface1;
			Account = account;
			Password = password;
		}

		public abstract void Redial();
	}
}

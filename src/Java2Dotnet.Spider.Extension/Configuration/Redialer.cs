using System;
using Java2Dotnet.Spider.Redial.Redialer;

namespace Java2Dotnet.Spider.Extension.Configuration
{
	public abstract class Redialer
	{
		[Flags]
		public enum Types
		{
			Adsl,
			H3C
		}

		public abstract Types Type { get; internal set; }

		public abstract IRedialer GetRedialer();
	}

	public class AdslRedialer : Redialer
	{
		public string Interface { get; set; } = "宽带连接";
		public string Account { get; set; }
		public string Password { get; set; }

		public override Types Type { get; internal set; } = Types.Adsl;

		public override IRedialer GetRedialer()
		{
			return new Redial.Redialer.AdslRedialer(Interface, Account, Password);
		}
	}

#if !NET_CORE
	public class H3CRedialer : Redialer
	{
		public string Sshhost;
		public string Sshuser;
		public string Sshpass;
		public string SshInterface;
		public int Sshport;
		public string Account { get; set; }
		public string Password { get; set; }

		public override Types Type { get; internal set; } = Types.H3C;

		public override IRedialer GetRedialer()
		{
			return new H3CSshAdslRedialer(Sshhost, Sshport, Sshuser, Sshpass, SshInterface, Account, Password);
		}
	}
#endif
}

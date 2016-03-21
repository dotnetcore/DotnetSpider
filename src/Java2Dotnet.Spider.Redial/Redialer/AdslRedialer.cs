using System;
using Java2Dotnet.Spider.Redial.Utils;

namespace Java2Dotnet.Spider.Redial.Redialer
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
			Console.WriteLine($"Try to redial: {Interface} {User} {Password}");
			AdslUtil.Connect(Interface, User, Password);
		}
	}
}

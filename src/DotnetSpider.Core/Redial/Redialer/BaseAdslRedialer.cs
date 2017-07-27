namespace DotnetSpider.Core.Redial.Redialer
{
	public abstract class BaseAdslRedialer : IRedialer
	{
		public string Interface { get; set; }
		public string Account { get; set; }
		public string Password { get; set; }

		protected BaseAdslRedialer(string interfaceName, string account, string password)
		{
			Interface = interfaceName;
			Account = account;
			Password = password;
		}

		public abstract void Redial();
	}
}

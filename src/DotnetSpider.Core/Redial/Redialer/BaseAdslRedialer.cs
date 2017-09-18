namespace DotnetSpider.Core.Redial.Redialer
{
	public abstract class BaseAdslRedialer : IRedialer
	{
		public abstract void Redial();

		public string Interface { get; protected set; }
		public string Account { get; protected set; }
		public string Password { get; protected set; }

		protected BaseAdslRedialer(string interfaceName, string account, string password)
		{
			Interface = interfaceName;
			Account = account;
			Password = password;
		}
	}
}

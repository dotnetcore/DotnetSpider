namespace DotnetSpider.Core.Redial.Redialer
{
	public abstract class BaseAdslRedialer : IRedialer
	{
		public abstract void Redial();

		public string Interface { get; }
		public string Account { get; }
		public string Password { get; }

		protected BaseAdslRedialer(string interfaceName, string account, string password)
		{
			Interface = interfaceName;
			Account = account;
			Password = password;
		}
	}
}

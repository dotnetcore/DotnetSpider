namespace DotnetSpider.Core.Redial.Redialer
{
    public abstract class BaseAdslRedialer : IRedialer
    {
        public abstract void Redial();

        protected string Interface { get; set; }
        protected string Account { get; set; }
        protected string Password { get; set; }

        protected BaseAdslRedialer(string interfaceName, string account, string password)
        {
            Interface = interfaceName;
            Account = account;
            Password = password;
        }

        protected BaseAdslRedialer()
        {
        }
    }
}
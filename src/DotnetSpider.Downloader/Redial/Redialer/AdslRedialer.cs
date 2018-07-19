namespace DotnetSpider.Downloader.Redial.Redialer
{
	/// <summary>
	/// 拨号器
	/// </summary>
	public abstract class AdslRedialer : IRedialer
	{
		/// <summary>
		/// 拨号
		/// </summary>
		public abstract void Redial();

		/// <summary>
		/// 网络接口名称
		/// </summary>
		protected string Interface { get; set; }

		/// <summary>
		/// 帐号
		/// </summary>
		protected string Account { get; set; }

		/// <summary>
		/// 密码
		/// </summary>
		protected string Password { get; set; }

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="interfaceName">网络接口名称</param>
		/// <param name="account">帐号</param>
		/// <param name="password">密码</param>
		protected AdslRedialer(string interfaceName, string account, string password)
		{
			Interface = interfaceName;
			Account = account;
			Password = password;
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		protected AdslRedialer()
		{
		}
	}
}

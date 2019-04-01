namespace DotnetSpider.Downloader
{
	/// <summary>
	/// 下载器代理选项
	/// </summary>
	public interface IDownloaderAgentOptions
	{
		/// <summary>
		/// 是否支持 ADSL 拨号
		/// </summary>
		bool SupportAdsl { get; }

		/// <summary>
		/// 是否忽略拨号，用于测试
		/// </summary>
		bool IgnoreRedialForTest { get; }

		/// <summary>
		/// 拨号间隔限制
		/// </summary>
		int RedialIntervalLimit { get; }

		/// <summary>
		/// 下载器代理标识
		/// </summary>
		string AgentId { get; }

		/// <summary>
		/// 下载器代理名称
		/// </summary>
		string Name { get; }

		/// <summary>
		/// ADSL 网络接口
		/// </summary>
		string AdslInterface { get; }

		/// <summary>
		/// ADSL 帐号
		/// </summary>
		string AdslAccount { get; }

		/// <summary>
		/// ADSL 密码
		/// </summary>
		string AdslPassword { get; }

		/// <summary>
		/// 代理供应接口
		/// </summary>
		string ProxySupplyUrl { get; }
	}
}
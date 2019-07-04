using DotnetSpider.DownloadAgent;

namespace DotnetSpider.Network
{
	/// <summary>
	/// 拨号器
	/// </summary>
	public abstract class AdslRedialerBase : IAdslRedialer
	{
		/// <summary>
		/// 拨号
		/// </summary>
		public abstract bool Redial();

		/// <summary>
		/// 配置
		/// </summary>
		protected DownloaderAgentOptions Options { get; }


		/// <summary>
		/// 构造方法
		/// </summary>
		protected AdslRedialerBase(DownloaderAgentOptions options)
		{
			Options = options;
		}
	}
}
using System.Net.NetworkInformation;
using System.Text;

namespace DotnetSpider.Network.InternetDetector
{
	/// <summary>
	/// 标准网络状态检测器, 通过PING某个网站是否能通
	/// </summary>
	public class DefaultInternetDetector : InternetDetectorBase
	{
		private const string TargetHost = "baidu.com";

		private static readonly PingOptions PingOptions = new PingOptions
		{
			DontFragment = true
		};

		/// <summary>
		/// 构造方法
		/// </summary>
		public DefaultInternetDetector()
		{
			Timeout = 3000;
		}

		/// <summary>
		/// 检测网络状态
		/// </summary>
		/// <returns>如果返回 True, 表示当前可以访问互联网</returns>
		protected override bool DoValidate()
		{
			try
			{
				var pingSender = new Ping();
				var buffer = Encoding.ASCII.GetBytes("hi");
				var reply = pingSender.Send(TargetHost, Timeout, buffer, PingOptions);
				return reply != null && reply.Status == IPStatus.Success;
			}
			catch
			{
				return false;
			}
		}
	}
}
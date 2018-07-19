

using System;
using System.Net.NetworkInformation;

namespace DotnetSpider.Downloader.Redial.InternetDetector
{
	/// <summary>
	/// 标准网络状态检测器, 通过PING某个网站是否能通
	/// </summary>
	public class DefaultInternetDetector : InternetDetector
	{
		private readonly string _url = "www.baidu.com";

		/// <summary>
		/// 构造方法
		/// </summary>
		public DefaultInternetDetector()
		{
			Timeout = 3000;
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="url">PING的目标链接</param>
		/// <param name="timeout">超时时间</param>
		public DefaultInternetDetector(string url = "www.baidu.com", int timeout = 10)
		{
			Timeout = timeout;
			if (!Uri.TryCreate(_url, UriKind.RelativeOrAbsolute, out var _))
			{
				throw new ArgumentException($"{url} is incorrect uri.");
			}
			_url = url;
		}

		/// <summary>
		/// 检测网络状态
		/// </summary>
		/// <returns>如果返回 True, 表示当前可以访问互联网</returns>
		protected override bool DoValidate()
		{
			try
			{
				Ping p = new Ping();//创建Ping对象p
				PingReply pr = p.Send(_url, Timeout);//向指定IP或者主机名的计算机发送ICMP协议的ping数据包

				return (pr != null && pr.Status == IPStatus.Success);//如果ping成功
			}
			catch
			{
				return false;
			}
		}
	}
}

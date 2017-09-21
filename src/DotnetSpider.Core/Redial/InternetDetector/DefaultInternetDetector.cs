

using System.Net.NetworkInformation;

namespace DotnetSpider.Core.Redial.InternetDetector
{
	public class DefaultInternetDetector : BaseInternetDetector
	{
		public DefaultInternetDetector()
		{
			Timeout = 3000;
		}

		public DefaultInternetDetector(int maxWaitTime)
		{
			Timeout = maxWaitTime;
		}

		public override bool DoValidate()
		{
			try
			{
				Ping p = new Ping();//创建Ping对象p
				PingReply pr = p.Send("www.baidu.com", Timeout);//向指定IP或者主机名的计算机发送ICMP协议的ping数据包

				return (pr != null && pr.Status == IPStatus.Success);//如果ping成功
			}
			catch
			{
				return false;
			}
		}
	}
}

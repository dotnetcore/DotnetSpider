using System;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace Java2Dotnet.Spider.Redial.NetworkValidater
{
	public class DefalutNetworkValidater : INetworkValidater
	{
		public void Wait()
		{
			while (true)
			{
				try
				{

#if !NET_CORE
					Ping p = new Ping();//创建Ping对象p
					PingReply pr = p.Send("www.baidu.com", 30000);//向指定IP或者主机名的计算机发送ICMP协议的ping数据包

					if (pr != null && pr.Status == IPStatus.Success)//如果ping成功
					{
						return;
					}

#endif
					HttpClient clinet = new HttpClient();
					IAsyncResult asyncResult = clinet.GetStringAsync("http://www.baidu.com");
					if (!asyncResult.AsyncWaitHandle.WaitOne(2000))
					{
						continue;
					}
					if (((Task<string>)asyncResult).Result.Contains("<title>百度一下，你就知道</title>"))
					{
						return;
					}
					Thread.Sleep(100);
				}
				catch
				{
					// ignored
				}
			}
		}
	}
}

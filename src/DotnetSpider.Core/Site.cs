using System.Collections.Generic;
using System.Text;
using DotnetSpider.Core.Proxy;
using System.Net;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Infrastructure;
using System;

namespace DotnetSpider.Core
{
	/// <summary>
	/// 采集站点的信息配置
	/// </summary>
	public class Site
	{
		private Encoding _encoding = Encoding.UTF8;
		private string _encodingName;

		/// <summary>
		/// 代理池
		/// </summary>
		public IHttpProxyPool HttpProxyPool { get; set; }

		/// <summary>
		/// 设置全局的HTTP头, 下载器下载数据时会带上所有的HTTP头
		/// </summary>
		public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

		/// <summary>
		/// 配置下载器下载的内容是Json还是Html, 如果是Auto则会自动检测下载的内容, 建议设为Auto
		/// </summary>
		public ContentType ContentType { get; set; } = ContentType.Auto;

		/// <summary>
		/// 是否去除外链
		/// </summary>
		public bool RemoveOutboundLinks { get; set; }

		/// <summary>
		/// 采集目标的Domain, 如果RemoveOutboundLinks为True, 则Domain不同的链接会被排除, 如果RemoveOutboundLinks为False, 此值没有作用
		/// </summary>
		public string[] Domains { get; set; }

		/// <summary>
		/// 设置全局Cookie
		/// </summary>
		public Cookies Cookies { get; set; }

		/// <summary>
		/// 设置 User Agent 头
		/// </summary>
		public string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.106 Safari/537.36";

		/// <summary>
		/// 设置 Accept 头
		/// </summary>
		public string Accept { get; set; } = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";

		/// <summary>
		/// 设置是否下载文件、图片
		/// </summary>
		public bool DownloadFiles { get; set; }

		/// <summary>
		/// 设置站点的编码 
		/// 如果没有设值, 下载器会尝试自动识别编码
		/// </summary>
		public string EncodingName
		{
			get => _encodingName;
			set
			{
				if (_encodingName != value)
				{
					_encodingName = value;
					_encoding = Encoding.GetEncoding(_encodingName);
				}
			}
		}

		/// <summary>
		/// 使用何种编码读取下载的内容, 如果没有设置编码, 下载器会尝试自动识别编码。
		/// 通过设置EncodingName才能修改此值
		/// </summary>
		public Encoding Encoding => _encoding;

		public readonly List<Request> StartRequests = new List<Request>();

		/// <summary>
		/// 每处理完一个目标链接后停顿的时间, 单位毫秒 
		/// </summary>
		public int SleepTime { get; set; } = 100;

		/// <summary>
		/// 目标链接的最大重试次数
		/// </summary>
		public int CycleRetryTimes { get; set; } = 5;

		/// <summary>
		/// 目标服务器是否使用了Gzip压缩数据
		/// 默认实现的下载器会自动解压数据, 不需要依赖此值
		/// </summary>
		[Obsolete]
		public bool IsUseGzip { get; set; }

		/// <summary>
		/// 构造函数
		/// </summary>
		public Site()
		{
#if NET_CORE
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
		}

		/// <summary>
		/// 添加一个起始链接到当前站点 
		/// </summary>
		/// <param name="startUrl">起始链接</param>
		public void AddStartUrl(string startUrl)
		{
			AddStartRequest(new Request(startUrl, null));
		}

		/// <summary>
		/// 添加一个起始链接到当前站点 
		/// </summary>
		/// <param name="startUrl">起始链接</param>
		/// <param name="datas">链接对应的一些额外数据</param>
		public void AddStartUrl(string startUrl, IDictionary<string, dynamic> datas)
		{
			AddStartRequest(new Request(startUrl, datas));
		}

		/// <summary>
		/// 添加多个起始链接到当前站点 
		/// </summary>
		/// <param name="startUrls">起始链接</param>
		public void AddStartUrls(IEnumerable<string> startUrls)
		{
			foreach (var url in startUrls)
			{
				AddStartUrl(url);
			}
		}

		/// <summary>
		/// 添加一个请求对象到当前站点
		/// </summary>
		/// <param name="startRequest">请求对象</param>
		public void AddStartRequest(Request startRequest)
		{
			lock (this)
			{
				StartRequests.Add(startRequest);
			}
		}

		/// <summary>
		/// 清空所有起始链接
		/// </summary>
		public void ClearStartRequests()
		{
			lock (this)
			{
				StartRequests.Clear();
			}
		}

		/// <summary>
		/// 添加一个全局的HTTP请求头 
		/// </summary>
		public void AddHeader(string key, string value)
		{
			if (Headers.ContainsKey(key))
			{
				Headers[key] = value;
			}
			else
			{
				Headers.Add(key, value);
			}
		}

		/// <summary>
		/// 获取HTTP代理
		/// </summary>
		/// <returns>HTTP代理对象</returns>
		public UseSpecifiedUriWebProxy GetHttpProxy()
		{
			return HttpProxyPool?.GetProxy();
		}

		/// <summary>
		/// 返回HTTP代理
		/// </summary>
		/// <param name="proxy">HTTP代理对象</param>
		/// <param name="statusCode">使用此HTTP对象下载数据后的状态码</param>
		public void ReturnHttpProxy(UseSpecifiedUriWebProxy proxy, HttpStatusCode statusCode)
		{
			HttpProxyPool?.ReturnProxy(proxy, statusCode);
		}

		/// <summary>
		/// 获取或设置全局的Cookie
		/// </summary>
		public string CookiesStringPart
		{
			set
			{
				if (Cookies == null)
				{
					Cookies = new Cookies();
				}
				Cookies.StringPart = value;
			}
			get
			{
				return Cookies == null ? string.Empty : Cookies.StringPart;
			}
		}

		/// <summary>
		/// 设置全局的Cookie
		/// </summary>
		public void SetCookies(Dictionary<string, string> cookies)
		{
			if (Cookies == null)
			{
				Cookies = new Cookies();
			}
			Cookies.PairPart = cookies;
		}
	}
}
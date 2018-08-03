using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DotnetSpider.Common
{
	public class Site : IDisposable
	{
		/// <summary>
		/// 设置全局的 HTTP 头, 下载器下载数据时会带上所有的 HTTP 头.
		/// </summary>
		public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

		/// <summary>
		/// 设置请求的内容是Json还是Html, 如果是Auto则会自动检测, 建议设为Auto.
		/// </summary>
		public ContentType ContentType { get; set; } = ContentType.Auto;

		/// <summary>
		/// 是否去除外链
		/// </summary>
		public bool RemoveOutboundLinks { get; set; }

		/// <summary>
		/// 采集目标的 Domain, 如果 RemoveOutboundLinks 为True, 则 Domain 不同的链接会被排除, 如果 RemoveOutboundLinks 为False, 此值没有作用.
		/// </summary>
		public string[] Domains { get; set; }

		/// <summary>
		/// User-Agent
		/// </summary>
		public string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.106 Safari/537.36";

		/// <summary>
		/// 设置 Accept 头
		/// </summary>
		public string Accept { get; set; }

		/// <summary>
		/// 设置站点的编码 
		/// 如果没有设值, 下载器会尝试自动识别编码
		/// </summary>
		public string EncodingName { get; set; }

		/// <summary>
		/// 每处理完一个目标链接后停顿的时间, 单位毫秒 
		/// </summary>
		public int SleepTime { get; set; } = 100;

		/// <summary>
		/// 目标链接的最大重试次数
		/// </summary>
		public int CycleRetryTimes { get; set; } = 5;

		/// <summary>
		/// 需要去除的 Json padding
		/// </summary>
		public string Padding { get; set; }

		/// <summary>
		/// 请求链接
		/// </summary>
		public List<Request> Requests { get; set; } = new List<Request>();

		/// <summary>
		/// 是否下载文件
		/// </summary>
		public bool DownloadFiles { get; set; }

		/// <summary>
		/// What mediatype should not be treated as file to download.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 定义哪些类型的内容不需要当成文件下载
		/// </summary>
		public List<string> ExcludeMediaTypes = new List<string>
		{
			"",
			"text/html",
			"text/plain",
			"text/richtext",
			"text/xml",
			"text/XML",
			"text/json",
			"text/javascript",
			"application/soap+xml",
			"application/xml",
			"application/json",
			"application/x-javascript",
			"application/javascript",
			"application/x-www-form-urlencoded"
		};

		/// <summary>
		/// 添加请求到当前站点
		/// </summary>
		/// <param name="requests">请求</param>
		public void AddRequests(params string[] requests)
		{
			if (requests == null || requests.Length == 0)
			{
				return;
			}
			AddRequests((IEnumerable<string>)requests);
		}

		/// <summary>
		/// 添加请求到当前站点
		/// </summary>
		/// <param name="requests">请求</param>
		public void AddRequests(IEnumerable<string> requests)
		{
			if (requests == null)
			{
				return;
			}
			foreach (var request in requests)
			{
				AddRequests(new Request(request, null));
			}
		}

		/// <summary>
		/// 添加请求到当前站点
		/// </summary>
		/// <param name="requests">请求</param>
		public void AddRequests(params Request[] requests)
		{
			if (requests == null || requests.Length == 0)
			{
				return;
			}
			AddRequests((IEnumerable<Request>)requests);
		}

		/// <summary>
		/// 添加请求到当前站点
		/// </summary>
		/// <param name="requests">请求对象</param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void AddRequests(IEnumerable<Request> requests)
		{
			if (requests == null)
			{
				return;
			}
			foreach (var request in requests)
			{
				request.Site = this;
				Requests.Add(request);
			}
		}

		/// <summary>
		/// 添加一个全局的HTTP请求头 
		/// </summary>
		public void AddHeader(string key, string value)
		{
			if (Headers == null)
			{
				Headers = new Dictionary<string, string>();
			}
			if (Headers.ContainsKey(key))
			{
				Headers[key] = value;
			}
			else
			{
				Headers.Add(key, value);
			}
		}

		public void Dispose()
		{
			Requests?.Clear();
		}
	}
}
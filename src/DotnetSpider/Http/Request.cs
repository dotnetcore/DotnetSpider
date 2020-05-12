using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotnetSpider.Infrastructure;
using MessagePack;

namespace DotnetSpider.Http
{
	/// <summary>
	/// 请求
	/// </summary>
	[Serializable]
	public class Request
	{
		[IgnoreMember] private RequestContent _content;

		public string Hash { get; set; }

		/// <summary>
		/// 数据存储字典
		/// </summary>
		public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

		/// <summary>
		/// Headers
		/// </summary>
		public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

		/// <summary>
		/// 任务标识
		/// </summary>
		public string Owner { get; set; }

		/// <summary>
		/// 下载代理标识
		/// </summary>
		public string Agent { get; set; }

		/// <summary>
		/// 下载代理类型
		/// </summary>
		public string DownloaderType { get; set; } = "HttpClient";

		/// <summary>
		/// 链接的深度
		/// </summary>
		public uint Depth { get; internal set; }

		/// <summary>
		/// 请求链接
		/// </summary>
		public Uri RequestUri { get; set; }

		/// <summary>
		/// 自动跳转
		/// </summary>
		public bool AutoRedirect { get; set; }

		/// <summary>
		/// 已经重试的次数
		/// </summary>
		public int RequestedTimes { get; internal set; }

		/// <summary>
		/// 请求的方法
		/// </summary>
		public string Method { get; set; } = "GET";

		/// <summary>
		/// 创建时间
		/// </summary>
		public long Timestamp { get; set; }

		/// <summary>
		/// 内容
		/// </summary>
		public byte[] Content { get; set; }

		/// <summary>
		/// 下载策略
		/// </summary>
		public RequestPolicy Policy { get; set; }

		public string Proxy { get; set; }

		[IgnoreMember]
		public string RedialRegExp
		{
			get => GetProperty(Consts.RedialRegexExpression);
			set => SetProperty(Consts.RedialRegexExpression, value);
		}

		/// <summary>
		/// User-Agent
		/// </summary>
		[IgnoreMember]
		public string UserAgent
		{
			get => GetHeader(HeaderNames.UserAgent);
			set => SetHeader(HeaderNames.UserAgent, value);
		}

		/// <summary>
		/// 请求链接时Referer参数的值
		/// </summary>
		[IgnoreMember]
		public string Referer
		{
			get => GetHeader(HeaderNames.Referer);
			set => SetHeader(HeaderNames.Referer, value);
		}

		/// <summary>
		/// 请求链接时Origin参数的值
		/// </summary>
		[IgnoreMember]
		public string Origin
		{
			get => GetHeader(HeaderNames.Origin);
			set => SetHeader(HeaderNames.Origin, value);
		}

		/// <summary>
		/// Accept
		/// </summary>
		[IgnoreMember]
		public string Accept
		{
			get => GetHeader(HeaderNames.Accept);
			set => SetHeader(HeaderNames.Accept, value);
		}

		/// <summary>
		/// 设置 Cookie
		/// </summary>
		[IgnoreMember]
		public string Cookie
		{
			get => GetHeader(HeaderNames.Cookie);
			set => SetHeader(HeaderNames.Cookie, value);
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		public Request()
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="url">链接</param>
		/// <param name="propertyDict">额外属性</param>
		public Request(string url, IDictionary<string, string> propertyDict = null)
			: this(new Uri(url, UriKind.RelativeOrAbsolute), propertyDict)
		{
		}

		public Request(Uri uri, IDictionary<string, string> propertyDict = null)
		{
			uri.NotNull(nameof(uri));
			RequestUri = uri;
			SetProperty(propertyDict);
		}

		public void SetContent(RequestContent content)
		{
			content.NotNull(nameof(content));
			_content = content;
			Content = content.ToBytes();
		}

		/// <summary>
		/// 设置此链接的额外信息
		/// </summary>
		/// <param name="key">键值</param>
		/// <param name="value">额外信息</param>
		public void SetProperty(string key, string value)
		{
			key.NotNullOrWhiteSpace(nameof(key));

			Properties[key] = value;
		}

		/// <summary>
		/// 添加属性
		/// </summary>
		/// <param name="dict">属性</param>
		public void SetProperty(IDictionary<string, string> dict)
		{
			if (dict == null)
			{
				return;
			}

			foreach (var kv in dict)
			{
				SetProperty(kv.Key, kv.Value);
			}
		}

		/// <summary>
		/// 获取属性
		/// </summary>
		/// <param name="key">Key</param>
		/// <returns></returns>
		public string GetProperty(string key)
		{
			return Properties.ContainsKey(key) ? Properties[key] : null;
		}

		public void SetHeader(string header, string value)
		{
			header.NotNullOrWhiteSpace(nameof(header));
			value.NotNullOrWhiteSpace(nameof(value));

			if (Headers.ContainsKey(header))
			{
				Headers[header] = value.Trim();
			}
			else
			{
				Headers.Add(header, value.Trim());
			}
		}

		public string GetHeader(string header)
		{
			header.NotNullOrWhiteSpace(nameof(header));
			return Headers.ContainsKey(header) ? Headers[header] : null;
		}

		public RequestContent GetContentObject()
		{
			if (_content != null)
			{
				return _content;
			}

			if (Content == null || Content.Length == 0)
			{
				return null;
			}

			using (var memory = new MemoryStream(Content))
			{
				return (RequestContent)MessagePackSerializer.Typeless.Deserialize(memory);
			}
		}

		public virtual string ComputeHash()
		{
			// Agent 不需要添加的原因是，每当 Request 再次添加到 Scheduler 前 Requested +1 已经导致 Hash 变化
			var obj = new HashObject
			{
				Owner = Owner,
				RequestUri = RequestUri,
				Method = Method,
				RequestedTime = RequestedTimes,
				Content = Content?.ToArray()
			};
			var bytes = MessagePackSerializer.Typeless.Serialize(obj);
			return bytes.ToMd5();
		}

		public override string ToString()
		{
			return $"Method: {Method} URL: {RequestUri}, Requested: {RequestedTimes}";
		}

		public Request Clone()
		{
			var request = new Request
			{
				Owner = Owner,
				Agent = Agent,
				DownloaderType = DownloaderType,
				Depth = Depth,
				RequestUri = new Uri(RequestUri.ToString(), UriKind.RelativeOrAbsolute),
				AutoRedirect = AutoRedirect,
				RequestedTimes = RequestedTimes,
				Method = Method,
				Timestamp = Timestamp,
				Content = Content?.ToArray(),
				Policy = Policy,
				Hash = Hash
			};
			foreach (var kv in Properties)
			{
				request.SetProperty(kv.Key, kv.Value);
			}

			foreach (var kv in Headers)
			{
				request.SetHeader(kv.Key, kv.Value);
			}

			return request;
		}

		public Request CreateNew(string uri)
		{
			uri.NotNullOrWhiteSpace(nameof(uri));
			var request = Clone();
			request.Depth += 1;
			request.Hash = null;
			request.RequestUri = new Uri(uri, UriKind.RelativeOrAbsolute);
			return request;
		}

		public class HashObject
		{
			public string Owner { get; set; }
			public Uri RequestUri { get; set; }
			public int RequestedTime { get; set; }
			public string Method { get; set; }
			public byte[] Content { get; set; }
		}
	}
}

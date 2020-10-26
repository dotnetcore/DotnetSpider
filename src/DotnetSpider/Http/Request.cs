using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using DotnetSpider.Extensions;
using DotnetSpider.Infrastructure;
using MessagePack;
using Newtonsoft.Json;

namespace DotnetSpider.Http
{
	/// <summary>
	/// 请求
	/// </summary>
	[Serializable]
	public class Request : IDisposable
	{
		private static HashSet<string> _hashBodyMethods = new HashSet<string> {"DELETE", "POST", "PATCH", "PUT"};

		private string _method;
		private Uri _requestUri;
		private RequestHeaders _headers;
		private Version _version;
		private object _content;

		private bool _disposed;
		private IDictionary<string, object> _properties;

		/// <summary>
		/// 请求的哈希
		/// </summary>
		public string Hash { get; set; }

		/// <summary>
		/// 任务标识
		/// </summary>
		public string Owner { get; set; }

		/// <summary>
		/// 请求的 Timeout 时间
		/// </summary>
		public int Timeout { get; set; } = 30000;

		/// <summary>
		/// 下载代理标识
		/// </summary>
		public string Agent { get; set; }

		/// <summary>
		/// 下载代理类型
		/// </summary>
		public string Downloader { get; set; }

		/// <summary>
		/// 链接的深度
		/// </summary>
		public int Depth { get; set; }

		/// <summary>
		/// 已经重试的次数
		/// </summary>
		public int RequestedTimes { get; set; }

		/// <summary>
		/// 创建时间
		/// </summary>
		public long Timestamp { get; set; }

		/// <summary>
		/// 下载策略
		/// </summary>
		public RequestPolicy Policy { get; set; }

		public Version Version
		{
			get => _version;
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				_version = value;
			}
		}

		[JsonIgnore]
		public object Content
		{
			get => _content;
			set =>
				_content = value switch
				{
					null => null,
					IHttpContent _ => value,
					_ => throw new ArgumentException("Content must be a IHttpContent")
				};
		}

		public string Method
		{
			get => _method;
			set => _method = value ?? throw new ArgumentNullException(nameof(value));
		}

		public Uri RequestUri
		{
			get => _requestUri;
			set
			{
				/*if (value != null && value.IsAbsoluteUri && !UriUtilities.IsHttpUri(value))
				{
					throw new ArgumentException($"http base address required: {value}");
				}*/

				_requestUri = value;
			}
		}

		public RequestHeaders Headers => _headers ??= new RequestHeaders();

		public IDictionary<string, object> Properties => _properties ??= new Dictionary<string, object>();

		[IgnoreMember, JsonIgnore]
		// ReSharper disable once InconsistentNaming
		public string PPPoERegex
		{
			get => _properties.ContainsKey(Const.PPPoEPattern) ? _properties[Const.PPPoEPattern]?.ToString() : null;
			set
			{
				if (_properties.ContainsKey(Const.PPPoEPattern))
				{
					_properties[Const.PPPoEPattern] = value;
				}
				else
				{
					_properties.Add(Const.PPPoEPattern, value);
				}
			}
		}

		/// <summary>
		/// 设置 Cookie
		/// </summary>
		[IgnoreMember, JsonIgnore]
		public string Cookie
		{
			get => Headers.Cookie;
			set => Headers.Cookie = value;
		}

		public Request()
			: this(null)
		{
		}

		public Request(string requestUri)
			: this(requestUri, null)
		{
		}

		public Request(string requestUri, Dictionary<string, object> properties = null)
			: this("Get", requestUri, properties)
		{
		}

		public Request(string method = "GET", string requestUri = null,
			Dictionary<string, object> properties = null) : this(method,
			string.IsNullOrEmpty(requestUri) ? null : new Uri(requestUri, UriKind.RelativeOrAbsolute), properties)
		{
		}


		public Request(string method = "GET", Uri requestUri = null, Dictionary<string, object> properties = null)
		{
			InitializeValues(method, requestUri);

			if (properties != null)
			{
				foreach (var property in properties)
				{
					Properties.Add(property.Key, property.Value);
				}
			}
		}

		public HttpRequestMessage ToHttpRequestMessage()
		{
			var httpRequestMessage =
				new HttpRequestMessage(
					string.IsNullOrWhiteSpace(Method)
						? HttpMethod.Get
						: new HttpMethod(Method.ToUpper()),
					RequestUri);

			foreach (var header in Headers)
			{
				httpRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
			}

			if (Headers.UserAgent.IsNullOrEmpty())
			{
				httpRequestMessage.Headers.TryAddWithoutValidation(HeaderNames.UserAgent,
					"Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_3) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.149 Safari/537.36 Edg/80.0.361.69");
			}

			if (Content != null && _hashBodyMethods.Contains(Method.ToUpper()))
			{
				HttpContent httpContent;
				if (Content is StringContent stringContent)
				{
					var encoding = Encoding.GetEncoding(stringContent.EncodingName);
					httpContent = new System.Net.Http.StringContent(
						stringContent.Content, encoding, stringContent.MediaType);
				}
				else if (Content is ByteArrayContent byteArrayContent && byteArrayContent.Bytes != null)
				{
					httpContent = new System.Net.Http.ByteArrayContent(byteArrayContent.Bytes);
				}
				else
				{
					throw new NotSupportedException(
						$"Not supported http content: {Content.GetType().FullName}");
				}

				if (Content is IHttpContent requestContent)
				{
					foreach (var header in requestContent.Headers)
					{
						httpRequestMessage.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
					}
				}

				httpRequestMessage.Content = httpContent;
			}

			return httpRequestMessage;
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("Method: ");
			sb.Append(_method);
			sb.Append(", RequestUri: '");
			sb.Append(_requestUri == (Uri)null ? "<null>" : _requestUri.ToString());
			sb.Append("', Version: ");
			sb.Append(_version);
			sb.Append(", Content: ");
			sb.Append(Content == null ? "<null>" : Content.GetType().ToString());
			sb.AppendLine(", Headers:");
			HeaderUtilities.DumpHeaders(sb, _headers, (Content as IHttpContent)?.Headers);
			return sb.ToString();
		}

		

		public Request Clone()
		{
			var request = new Request
			{
				Owner = Owner,
				Agent = Agent,
				Downloader = Downloader,
				Depth = Depth,
				RequestUri = RequestUri,
				Method = Method,
				// 是否需要复制
				Timestamp = Timestamp,
				Content = (Content as IHttpContent)?.Clone(),
				Policy = Policy,
				RequestedTimes = RequestedTimes,
				Hash = Hash,
				Version = Version,
				Timeout = Timeout
			};
			foreach (var kv in Properties)
			{
				request.Properties.Add(kv.Key, kv.Value);
			}

			foreach (var kv in Headers)
			{
				request.Headers.Add(kv.Key, kv.Value);
			}

			return request;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposing || _disposed)
			{
				return;
			}

			_disposed = true;

			_headers?.Clear();
			_properties?.Clear();

			(Content as IDisposable)?.Dispose();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void InitializeValues(
			string method, Uri requestUri)
		{
			if (string.IsNullOrWhiteSpace(method))
			{
				throw new ArgumentNullException(nameof(method));
			}

			/*if (requestUri != null && requestUri.IsAbsoluteUri && !UriUtilities.IsHttpUri(requestUri))
			{
				throw new ArgumentException("http base address is required", nameof(requestUri));
			}*/

			_properties = new Dictionary<string, object>();
			_method = method;
			_requestUri = requestUri;
			_version = HttpVersion.Version11;
		}
	}
}

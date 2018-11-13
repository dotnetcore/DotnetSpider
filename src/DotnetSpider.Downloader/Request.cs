using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

namespace DotnetSpider.Downloader
{
	/// <summary>
	/// 链接请求
	/// </summary>
	public class Request : IDisposable
	{
		private string _url;

		#region Headers

		/// <summary>
		/// User-Agent
		/// </summary>
		public string UserAgent { get; set; } =
			"Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.81 Safari/537.36";

		/// <summary>
		/// 请求链接时Referer参数的值
		/// </summary>
		public string Referer { get; set; }

		/// <summary>
		/// 请求链接时Origin参数的值
		/// </summary>
		public string Origin { get; set; }

		/// <summary>
		/// Accept
		/// </summary>
		public string Accept { get; set; }

		/// <summary>
		/// 仅在发送 POST 请求时需要设置
		/// </summary>
		public string ContentType { get; set; }

		/// <summary>
		/// Headers
		/// </summary>
		public Dictionary<string, object> Headers { get; set; } = new Dictionary<string, object>();

		#endregion

		/// <summary>
		/// 字符编码
		/// </summary>
		public string EncodingName { get; set; }

		/// <summary>
		/// 请求链接的方法
		/// </summary>
		public HttpMethod Method { get; set; } = HttpMethod.Get;

		/// <summary>
		/// 存储此链接对应的额外数据字典
		/// </summary>
		public readonly Dictionary<string, dynamic> Properties = new Dictionary<string, dynamic>();

		/// <summary>
		/// 请求此链接时需要POST的数据
		/// </summary>
		public string Content { get; set; }

		/// <summary>
		/// 如果是 POST 请求, 可以设置压缩模式上传数据
		/// </summary>
		public CompressMode CompressMode { get; set; }

		/// <summary>
		/// 请求链接, 不使用 Uri 的原因是可能引起多重编码的问题
		/// </summary>
		public string Url
		{
			get => _url;
			set
			{
				_url = value;
				RequestUri = new Uri(_url);
			}
		}

		[JsonIgnore]
		public Uri RequestUri { get; private set; }

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
		public Request(string url) : this(url, null)
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="url">链接</param>
		/// <param name="properties">额外属性</param>
		public Request(string url, Dictionary<string, object> properties = null)
		{
			Url = url;
			if (properties != null)
			{
				Properties = properties;
			}
		}

		/// <summary>
		/// 设置此链接的额外信息
		/// </summary>
		/// <param name="key">键值</param>
		/// <param name="value">额外信息</param>
		public void AddProperty(string key, dynamic value)
		{
			if (null == key)
			{
				return;
			}

			if (Properties.ContainsKey(key))
			{
				Properties[key] = value;
			}
			else
			{
				Properties.Add(key, value);
			}
		}

		public dynamic GetProperty(string key)
		{
			return Properties.ContainsKey(key) ? Properties[key] : null;
		}

		public void AddHeader(string key, object value)
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
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			if (this == obj) return true;
			if (obj == null || GetType() != obj.GetType()) return false;

			Request request = (Request) obj;

			if (!Equals(Referer, request.Referer)) return false;
			if (!Equals(Origin, request.Origin)) return false;
			if (!Equals(Method, request.Method)) return false;
			if (!Equals(Content, request.Content)) return false;

			if (Properties.Count != request.Properties.Count) return false;

			foreach (var entry in Properties)
			{
				if (!request.Properties.ContainsKey(entry.Key)) return false;
				if (!Equals(entry.Value, request.Properties[entry.Key])) return false;
			}

			var headersCount = Headers?.Count ?? 0;
			var requestHeadersCount = request.Headers?.Count ?? 0;
			if (headersCount != requestHeadersCount) return false;
			if (headersCount == requestHeadersCount && headersCount == 0) return true;
			if (Headers == null || request.Headers == null) return true;
			foreach (var header in Headers)
			{
				if (!request.Headers.ContainsKey(header.Key)) return false;
				if (!Equals(header.Value, request.Headers[header.Key])) return false;
			}

			return true;
		}

		/// <summary>
		/// Gets the System.Type of the current instance.
		/// </summary>
		/// <returns>The exact runtime type of the current instance.</returns>
		public override int GetHashCode()
		{
			return JsonConvert.SerializeObject(this).GetHashCode();
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Properties.Clear();
		}

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>A string that represents the current object.</returns>
		public override string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}

		public virtual Request Clone()
		{
			return (Request) MemberwiseClone();
		}
	}
}
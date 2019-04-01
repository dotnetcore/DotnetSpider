using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net.Http;
using DotnetSpider.Core;
using Newtonsoft.Json;

namespace DotnetSpider.Downloader
{
	/// <summary>
	/// 请求
	/// </summary>
	public class Request
	{
		private readonly Dictionary<string, string> _properties = new Dictionary<string, string>();

		/// <summary>
		/// 请求的 HASH 值
		/// </summary>
		public string Hash { get; set; }

		/// <summary>
		/// 任务标识
		/// </summary>
		public string OwnerId { get; set; }

		/// <summary>
		/// 下载器代理标识
		/// </summary>
		public string AgentId { get; set; }

		/// <summary>
		/// 链接的深度，用户不得修改
		/// </summary>
		public int Depth { get; set; }

		/// <summary>
		/// 请求链接
		/// </summary>
		public string Url { get; set; }

		/// <summary>
		/// 判断是否需要切换 IP 的正则表达式
		/// </summary>
		public string ChangeIpPattern { get; set; }

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
		/// 设置 Cookie
		/// </summary>
		public string Cookie { get; set; }

		/// <summary>
		/// Headers
		/// </summary>
		public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

		#endregion

		/// <summary>
		/// 字符编码
		/// </summary>
		public string Encoding { get; set; }

		/// <summary>
		/// 请求的 Body
		/// </summary>
		public string Body { get; set; }

		/// <summary>
		/// 请求的方法
		/// </summary>
		public HttpMethod Method { get; set; } = HttpMethod.Get;

		/// <summary>
		/// 已经重试的次数
		/// </summary>
		public int RetriedTimes { get; set; }

		/// <summary>
		/// 是否需要用压缩方法发送 Body
		/// </summary>
		public Compression Compression { get; set; }

		/// <summary>
		/// 获取当前请求的所有属性
		/// </summary>
		public IDictionary<string, string> GetProperties() => _properties.ToImmutableDictionary();

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
		public Request(string url, IDictionary<string, string> properties = null)
		{
			Url = url;
			AddProperty(properties);
		}

		/// <summary>
		/// 设置此链接的额外信息
		/// </summary>
		/// <param name="key">键值</param>
		/// <param name="value">额外信息</param>
		public void AddProperty(string key, string value)
		{
			if (null == key)
			{
				return;
			}

			if (_properties.ContainsKey(key))
			{
				_properties[key] = value;
			}
			else
			{
				_properties.Add(key, value);
			}
		}

		/// <summary>
		/// 添加属性
		/// </summary>
		/// <param name="dict">属性</param>
		public void AddProperty(IDictionary<string, string> dict)
		{
			if (null == dict)
			{
				return;
			}

			foreach (var kv in dict)
			{
				AddProperty(kv.Key, kv.Value);
			}
		}

		/// <summary>
		/// 获取属性
		/// </summary>
		/// <param name="key">Key</param>
		/// <returns></returns>
		public string GetProperty(string key)
		{
			return _properties.ContainsKey(key) ? _properties[key] : null;
		}

		/// <summary>
		/// 设置请求头
		/// </summary>
		/// <param name="key">请求头</param>
		/// <param name="value">请求值</param>
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
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			_properties.Clear();
		}

		/// <summary>
		/// Hash 放在客户端计算的原因是调度器可能是分布式的。不能随意去调整服务端的代码。
		/// </summary>
		public virtual void ComputeHash()
		{
			// TODO:
			var content = $"{OwnerId}{Url}{Method}{Body}{Cookie}";
			Hash = content.ToMd5();
		}
	}
}
using System;
using System.Collections.Generic;
using DotnetSpider.Core.Infrastructure;
using System.Net;
using Newtonsoft.Json;
using System.Net.Http;
using DotnetSpider.Core.Proxy;

namespace DotnetSpider.Core
{
	/// <summary>
	/// 对爬取链接信息的封装
	/// </summary>
	public class Request : IDisposable
	{
		private readonly object _locker = new object();
		private string _url;
		private Uri _uri;

		/// <summary>
		/// 站点信息
		/// </summary>
		[JsonIgnore]
		public Site Site { get; internal set; }

		/// <summary>
		/// 此链接解析出的数据结果数量
		/// </summary>
		[JsonIgnore]
		public int? CountOfResults { get; set; }

		/// <summary>
		/// 所有数据结果插入数据库后实际增加或更新的数量
		/// </summary>
		[JsonIgnore]
		public int? EffectedRows { get; set; }

		/// <summary>
		/// 下载器分组
		/// </summary>
		public int? DownloaderGroup { get; set; }

		/// <summary>
		/// 下载此链接内容时使用的代理
		/// </summary>
		[JsonIgnore]
		public UseSpecifiedUriWebProxy Proxy { get; set; }

		/// <summary>
		/// 当前链接的深度, 默认构造的链接深度为1, 用于控制爬取的深度
		/// </summary>
		public int Depth { get; set; } = 1;

		/// <summary>
		/// 当前链接子链接的深度
		/// </summary>
		[JsonIgnore]
		public int NextDepth => Depth + 1;

		/// <summary>
		/// 当前链接已经重试的次数
		/// </summary>
		public int CycleTriedTimes { get; set; }

		/// <summary>
		/// 当前链接是否是合法链接
		/// </summary>
		[JsonIgnore]
		public bool IsAvailable
		{
			get
			{
				if (string.IsNullOrEmpty(Url) || string.IsNullOrWhiteSpace(Url))
				{
					return false;
				}
				if (Url.Length < 6)
				{
					return false;
				}
				var schema = Url.Substring(0, 5).ToLower();
				if (!schema.StartsWith("http") && !schema.StartsWith("https"))
				{
					return false;
				}
				return true;
			}
		}

		/// <summary>
		/// 请求链接时Referer参数的值
		/// </summary>
		public string Referer { get; set; }

		/// <summary>
		/// 请求链接时Origin参数的值
		/// </summary>
		public string Origin { get; set; }

		/// <summary>
		/// 请求链接的方法
		/// </summary>
		public HttpMethod Method { get; set; } = HttpMethod.Get;

		/// <summary>
		/// 链接的优先级, 仅用于优先级队列
		/// </summary>
		public int Priority { get; set; }

		/// <summary>
		/// 存储此链接对应的额外数据字典
		/// </summary>
		public Dictionary<string, dynamic> Extras { get; set; }

		/// <summary>
		/// 请求此链接时需要POST的数据
		/// </summary>
		public string PostBody { get; set; }

		/// <summary>
		/// 请求链接
		/// </summary>
		public string Url
		{
			get { return _url; }
			set
			{
				if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
				{
					_url = null;
					return;
				}
				if (Uri.TryCreate(value.TrimEnd('#'), UriKind.RelativeOrAbsolute, out _uri))
				{
					_url = _uri.ToString();
				}
				else
				{
					_url = null;
				}
			}
		}

		/// <summary>
		/// 请求链接
		/// </summary>
		[JsonIgnore]
		public Uri Uri
		{
			get
			{
				return _uri;
			}
		}

		/// <summary>
		/// TODO 此链接信息的唯一标识, 可能需要添加更多属性, 如某些场景URL是完成一致, 使用Referer或者Cookie来区别请求
		/// </summary>
		[JsonIgnore]
		public string Identity => CryptoUtil.Md5Encrypt(Url + PostBody);

		/// <summary>
		/// 请求此链接后服务器返回的状态码
		/// </summary>
		[JsonIgnore]
		public HttpStatusCode? StatusCode { get; set; }

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
		/// <param name="extras">数据字典</param>
		public Request(string url, IDictionary<string, dynamic> extras = null)
		{
			Url = url;

			if (string.IsNullOrEmpty(Url))
			{
				return;
			}

			if (extras != null)
			{
				foreach (var extra in extras)
				{
					PutExtra(extra.Key, extra.Value);
				}
			}
		}

		/// <summary>
		/// 通过键值取得此链接对应的额外信息
		/// </summary>
		/// <param name="key">键值</param>
		/// <returns>额外信息</returns>
		public dynamic GetExtra(string key)
		{
			lock (_locker)
			{
				if (Extras == null)
				{
					return null;
				}

				if (Extras.ContainsKey(key))
				{
					return Extras[key];
				}
				return null;
			}
		}

		/// <summary>
		/// 设置此链接的额外信息
		/// </summary>
		/// <param name="key">键值</param>
		/// <param name="value">额外信息</param>
		public void PutExtra(string key, dynamic value)
		{
			lock (_locker)
			{
				if (key != null)
				{
					if (Extras == null)
					{
						Extras = new Dictionary<string, dynamic>();
					}

					if (Extras.ContainsKey(key))
					{
						Extras[key] = value;
					}
					else
					{
						Extras.Add(key, value);
					}
				}
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

			Request request = (Request)obj;

			if (!Url.Equals(request.Url)) return false;

			return true;
		}

		/// <summary>
		/// Gets the System.Type of the current instance.
		/// </summary>
		/// <returns>The exact runtime type of the current instance.</returns>
		public override int GetHashCode()
		{
			return Identity.GetHashCode();
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Extras.Clear();
		}

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>A string that represents the current object.</returns>
		public override string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}

		/// <summary>
		/// TODO 重新思考这个克隆方法是否还有作用
		/// </summary>
		/// <returns>对爬取链接信息的封装</returns>
		public Request Clone()
		{
			lock (_locker)
			{
				IDictionary<string, dynamic> extras = new Dictionary<string, dynamic>();
				if (Extras != null)
				{
					foreach (var entry in Extras)
					{
						extras.Add(entry.Key, entry.Value);
					}
				}
				Request newObj = new Request(Url, extras)
				{
					Method = Method,
					Priority = Priority,
					Referer = Referer,
					PostBody = PostBody,
					Origin = Origin,
					Depth = Depth,
					CycleTriedTimes = CycleTriedTimes,
					Proxy = Proxy,
					StatusCode = StatusCode
				};
				return newObj;
			}
		}
	}
}
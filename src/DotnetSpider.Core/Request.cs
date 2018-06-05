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
	/// Object contains url to crawl. 
	/// It contains some additional information. 
	/// </summary>
	public class Request : IDisposable
	{
		private readonly object _locker = new object();
        /// <summary>
        /// 此链接解析出的数据结果数量
        /// </summary>
        [JsonIgnore]
        internal int? CountOfResults { get; set; }
        /// <summary>
        /// 所有数据结果插入数据库后实际增加或更新的数量
        /// </summary>
        internal int? EffectedRows { get; set; }

		public UseSpecifiedUriWebProxy Proxy { get; set; }
        /// <summary>
        /// 当前链接的深度, 默认构造的链接深度为1, 用于控制爬取的深度
        /// </summary>
        public int Depth { get; set; } = 1;
        /// <summary>
        /// 当前链接已经重试的次数
        /// </summary>
        public int CycleTriedTimes { get; set; }
        /// <summary>
        /// 当前链接子链接的深度
        /// </summary>
        [JsonIgnore]
        public int NextDepth => Depth + 1;
        /// <summary>
        /// 当前链接是否是合法链接
        /// </summary>
        [JsonIgnore]
        public bool IsAvailable { get; } = true;
        /// <summary>
        /// 请求链接时Referer参数的值
        /// </summary>
        public string Referer { get; set; }
        /// <summary>
        /// 请求链接时Origin参数的值
        /// </summary>
        public string Origin { get; set; }

        /// <summary>
        /// 链接的优先级, 仅用于优先级队列
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// 存储此链接对应的额外数据字典
        /// </summary>
        public Dictionary<string, dynamic> Extras { get; set; }

		/// <summary>
		/// The http method of the request. Get for default.
		/// </summary>
		public HttpMethod Method { get; set; } = HttpMethod.Get;

		public string PostBody { get; set; }
        /// <summary>
        /// 请求链接, 请求链接限定为Uri的原因: 无论是本地文件资源或者网络资源都是可以用Uri来定义的
        /// 比如本地文件: file:///C:/Users/Lewis/Desktop/111.png
        /// </summary>
        public Uri Url { get; set; }
        /// <summary>
        /// TODO 此链接信息的唯一标识, 可能需要添加更多属性, 如某些场景URL是完成一致, 使用Referer或者Cookie来区别请求
        /// </summary>
        [JsonIgnore]
        public string Identity => Encrypt.Md5Encrypt(Url + PostBody);

		public HttpStatusCode? StatusCode { get; set; }

		public Request()
		{
		}

		public Request(string url) : this(url, null)
		{
		}

		public Request(string url, IDictionary<string, dynamic> extras = null)
		{
			if (string.IsNullOrEmpty(url))
			{
				IsAvailable = false;
				return;
			}
			if (Uri.TryCreate(url.TrimEnd('#'), UriKind.RelativeOrAbsolute, out var tmp))
			{
				Url = tmp;
			}
			else
			{
				IsAvailable = false;
				return;
			}
            //排除http协议
			if (Url.Scheme != "http" && Url.Scheme != "https")
			{
				IsAvailable = false;
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

		public bool ExistExtra(string key)
		{
			lock (_locker)
			{
				if (Extras == null)
				{
					return false;
				}

				return Extras.ContainsKey(key);
			}
		}

		public Request PutExtra(string key, dynamic value)
		{
			lock (_locker)
			{
				if (key == null)
					return this;
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

				return this;
			}
		}

		public override bool Equals(object o)
		{
			if (this == o) return true;
			if (o == null || GetType() != o.GetType()) return false;

			Request request = (Request)o;

			if (!Url.Equals(request.Url)) return false;

			return true;
		}

		public override int GetHashCode()
		{
			return Identity.GetHashCode();
		}

		public void Dispose()
		{
			Extras.Clear();
		}

		public override string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}

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

		private Request(Uri url, IDictionary<string, dynamic> extras = null)
		{
			Url = url;
			if (extras != null)
			{
				foreach (var extra in extras)
				{
					PutExtra(extra.Key, extra.Value);
				}
			}
		}
	}
}
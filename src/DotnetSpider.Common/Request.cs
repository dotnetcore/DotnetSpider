using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DotnetSpider.Common
{
	/// <summary>
	/// 链接请求
	/// </summary>
	public class Request : IDisposable
	{
		private string _url;

		/// <summary>
		/// 当前链接的深度, 默认构造的链接深度为1, 用于控制爬取的深度
		/// </summary>
		public int Depth { get; set; } = 1;

		/// <summary>
		/// 当前链接已经重试的次数
		/// </summary>
		public int CycleTriedTimes { get; set; }

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
		public Dictionary<string, dynamic> Properties { get; set; } = new Dictionary<string, dynamic>();

		/// <summary>
		/// 请求此链接时需要POST的数据
		/// </summary>
		public string Content { get; set; }

		/// <summary>
		/// 如果是 POST 请求, 可以设置压缩模式上传数据
		/// </summary>
		public CompressMode CompressMode { get; set; }

		/// <summary>
		/// 站点信息
		/// </summary>
		[JsonIgnore]
		public Site Site { get; set; }

		/// <summary>
		/// 请求链接, 请求链接限定为Uri的原因: 无论是本地文件资源或者网络资源都是可以用Uri来定义的
		/// 比如本地文件: file:///C:/Users/Lewis/Desktop/111.png
		/// </summary>
		[Required]
		public string Url
		{
			get => _url;
			set
			{
				_url = new Uri(value).ToString();
			}
		}

		public virtual string Identity => $"{Referer}.{Origin}.{Method}.{Content}.{Url}".ToShortMd5();

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
		public Request(string url, Dictionary<string, dynamic> properties = null)
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
			lock (this)
			{
				if (key != null)
				{
					if (Properties == null)
					{
						Properties = new Dictionary<string, dynamic>();
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

			if (!Equals(Depth, request.Depth)) return false;
			if (!Equals(CycleTriedTimes, request.CycleTriedTimes)) return false;
			if (!Equals(Referer, request.Referer)) return false;
			if (!Equals(Origin, request.Origin)) return false;
			if (!Equals(Method, request.Method)) return false;
			if (!Equals(Priority, request.Priority)) return false;
			if (!Equals(Content, request.Content)) return false;

			if (Properties == null)
			{
				Properties = new Dictionary<string, object>();
			}

			if (request.Properties == null)
			{
				request.Properties = new Dictionary<string, object>();
			}

			if (Properties.Count != request.Properties.Count) return false;

			foreach (var entry in Properties)
			{
				if (!request.Properties.ContainsKey(entry.Key)) return false;
				if (!Equals(entry.Value, request.Properties[entry.Key])) return false;
			}

			return true;
		}

		/// <summary>
		/// Gets the System.Type of the current instance.
		/// </summary>
		/// <returns>The exact runtime type of the current instance.</returns>
		public override int GetHashCode()
		{
			return ToString().GetHashCode();
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
			return (Request)MemberwiseClone();
		}
	}
}
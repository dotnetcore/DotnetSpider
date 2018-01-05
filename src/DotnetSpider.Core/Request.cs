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
	/// ����ȡ������Ϣ�ķ�װ
	/// </summary>
	public class Request : IDisposable
	{
		private readonly object _locker = new object();
		private string _url;
		private Uri _uri;

		/// <summary>
		/// վ����Ϣ
		/// </summary>
		[JsonIgnore]
		public Site Site { get; internal set; }

		/// <summary>
		/// �����ӽ����������ݽ������
		/// </summary>
		[JsonIgnore]
		public int? CountOfResults { get; set; }

		/// <summary>
		/// �������ݽ���������ݿ��ʵ�����ӻ���µ�����
		/// </summary>
		[JsonIgnore]
		public int? EffectedRows { get; set; }

		/// <summary>
		/// ����������
		/// </summary>
		public int? DownloaderGroup { get; set; }

		/// <summary>
		/// ���ش���������ʱʹ�õĴ���
		/// </summary>
		[JsonIgnore]
		public UseSpecifiedUriWebProxy Proxy { get; set; }

		/// <summary>
		/// ��ǰ���ӵ����, Ĭ�Ϲ�����������Ϊ1, ���ڿ�����ȡ�����
		/// </summary>
		public int Depth { get; set; } = 1;

		/// <summary>
		/// ��ǰ���������ӵ����
		/// </summary>
		[JsonIgnore]
		public int NextDepth => Depth + 1;

		/// <summary>
		/// ��ǰ�����Ѿ����ԵĴ���
		/// </summary>
		public int CycleTriedTimes { get; set; }

		/// <summary>
		/// ��ǰ�����Ƿ��ǺϷ�����
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
		/// ��������ʱReferer������ֵ
		/// </summary>
		public string Referer { get; set; }

		/// <summary>
		/// ��������ʱOrigin������ֵ
		/// </summary>
		public string Origin { get; set; }

		/// <summary>
		/// �������ӵķ���
		/// </summary>
		public HttpMethod Method { get; set; } = HttpMethod.Get;

		/// <summary>
		/// ���ӵ����ȼ�, ���������ȼ�����
		/// </summary>
		public int Priority { get; set; }

		/// <summary>
		/// �洢�����Ӷ�Ӧ�Ķ��������ֵ�
		/// </summary>
		public Dictionary<string, dynamic> Extras { get; set; }

		/// <summary>
		/// ���������ʱ��ҪPOST������
		/// </summary>
		public string PostBody { get; set; }

		/// <summary>
		/// ��������
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
		/// ��������
		/// </summary>
		[JsonIgnore]
		public Uri Uri => _uri;

		/// <summary>
		/// TODO ��������Ϣ��Ψһ��ʶ, ������Ҫ��Ӹ�������, ��ĳЩ����URL�����һ��, ʹ��Referer����Cookie����������
		/// </summary>
		[JsonIgnore]
		public string Identity => CryptoUtil.Md5Encrypt(Url + PostBody);

		/// <summary>
		/// ��������Ӻ���������ص�״̬��
		/// </summary>
		[JsonIgnore]
		public HttpStatusCode? StatusCode { get; set; }

		/// <summary>
		/// ���췽��
		/// </summary>
		public Request()
		{
		}

		/// <summary>
		/// ���췽��
		/// </summary>
		/// <param name="url">����</param>
		public Request(string url) : this(url, null)
		{
		}

		/// <summary>
		/// ���췽��
		/// </summary>
		/// <param name="url">����</param>
		/// <param name="extras">�����ֵ�</param>
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
		/// ͨ����ֵȡ�ô����Ӷ�Ӧ�Ķ�����Ϣ
		/// </summary>
		/// <param name="key">��ֵ</param>
		/// <returns>������Ϣ</returns>
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
		/// ���ô����ӵĶ�����Ϣ
		/// </summary>
		/// <param name="key">��ֵ</param>
		/// <param name="value">������Ϣ</param>
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
		/// TODO ����˼�������¡�����Ƿ�������
		/// </summary>
		/// <returns>����ȡ������Ϣ�ķ�װ</returns>
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
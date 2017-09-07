using System;
using System.Collections.Generic;
using DotnetSpider.Core.Infrastructure;
using System.Net;
using Newtonsoft.Json;
using System.Net.Http;

namespace DotnetSpider.Core
{
	/// <summary>
	/// Object contains url to crawl. 
	/// It contains some additional information. 
	/// </summary>
	public class Request : IDisposable
	{
		private readonly object _locker = new object();

		public const string CycleTriedTimes = "983009aebaee467b92cd44188da2b021";
		public const string StatusCodeKey = "02d71099b89749dda18055345fe9abfc";
		public const string Proxy = "6f09c4d6167a427282088a59bebdfe33";
		public const string ResultIsEmptyTriedTimes = "BA2788B8FC484B11861D524B5FB21582";

		public int Depth { get; internal set; } = 1;

		public int NextDepth => Depth + 1;

		public bool IsAvailable { get; } = true;

		public string Referer { get; set; }

		public string Origin { get; set; }

		/// <summary>
		/// Set the priority of request for sorting. 
		/// Need a scheduler supporting priority. 
		/// </summary>
		public int Priority { get; set; }

		/// <summary>
		/// Store additional information in extras.
		/// </summary>
		public Dictionary<string, dynamic> Extras { get; set; }

		/// <summary>
		/// The http method of the request. Get for default.
		/// </summary>
		public HttpMethod Method { get; set; } = HttpMethod.Get;

		public string PostBody { get; set; }

		public Uri Url { get; set; }

		public string Identity => Encrypt.Md5Encrypt(Url + PostBody);

		public HttpStatusCode? StatusCode
		{
			get
			{
				var code = GetExtra(StatusCodeKey);
				if (code == null)
				{
					return null;
				}
				else
				{
					return (HttpStatusCode)code;
				}
			}
			set => PutExtra(StatusCodeKey, value);
		}

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
					Depth = Depth
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
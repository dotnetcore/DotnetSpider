using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Java2Dotnet.Spider.Common;
using Newtonsoft.Json;

namespace Java2Dotnet.Spider.Core
{
	/// <summary>
	/// Object contains url to crawl. 
	/// It contains some additional information. 
	/// </summary>
	public class Request : IDisposable
#if !NET_CORE
	, ICloneable
#endif
	{
		public const string CycleTriedTimes = "983009ae-baee-467b-92cd-44188da2b021";
		public const string StatusCode = "02d71099-b897-49dd-a180-55345fe9abfc";
		public const string Proxy = "6f09c4d6-167a-4272-8208-8a59bebdfe33";

		public int Depth { get; set; }
		public int NextDepth => Depth + 1;

		public Request()
		{
		}

		public Request(string url, int grade, IDictionary<string, dynamic> extras) : this(new Uri(url), grade, extras)
		{
		}

		public Request(Uri url, int grade, IDictionary<string, dynamic> extras)
		{
			Url = url;

			if (extras != null)
			{
				foreach (var extra in extras)
				{
					PutExtra(extra.Key, extra.Value);
				}
			}

			Depth = grade;
		}

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
		public string Method { get; set; } = "GET";

		public string PostBody { get; set; }

		public Uri Url { get; set; }

		public dynamic GetExtra(string key)
		{
			lock (this)
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
			lock (this)
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
			lock (this)
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
			return $"Request {{ url='{Url}', method='{Method}', extras='{Extras}', priority='{Priority}'}}";
		}

		public string Identity => Encrypt.Md5Encrypt(Url + PostBody);

		public object Clone()
		{
			lock (this)
			{
				IDictionary<string, dynamic> extras = new Dictionary<string, dynamic>();
				if (Extras != null)
				{
					foreach (var entry in Extras)
					{
						extras.Add(entry.Key, entry.Value);
					}
				}
				Request newObj = new Request(Url, Depth, extras)
				{
					Method = Method,
					Priority = Priority,
					Referer = Referer,
					PostBody = PostBody,
					Origin = Origin
				};
				return newObj;
			}
		}
	}
}

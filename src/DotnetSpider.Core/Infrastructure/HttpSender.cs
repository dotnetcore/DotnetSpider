using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace DotnetSpider.Core.Infrastructure
{
	public static class HttpSender
	{
		/// <summary>  
		/// 根据相传入的数据，得到相应页面数据  
		/// </summary>  
		/// <param name="item">参数类对象</param>  
		/// <returns>返回HttpResult类型</returns>  
		public static HttpResult Request(HttpRequest item)
		{
			HttpResult result = new HttpResult();
			HttpWebRequest request;
			HttpWebResponse response;
			try
			{
				//准备参数  
				request = GenerateRequest(item);
			}
			catch (Exception ex)
			{
				result.Cookie = string.Empty;
				result.Header = null;
				result.Html = ex.Message;
				result.StatusDescription = "配置参数时出错：" + ex.Message;
				//配置参数时出错  
				return result;
			}
			try
			{
				//请求数据  
				using (response = (HttpWebResponse)request.GetResponse())
				{
					GetData(item, result, response);
				}
			}
			catch (WebException ex)
			{
				if (ex.Response != null)
				{
					using (response = (HttpWebResponse)ex.Response)
					{
						GetData(item, result, response);
					}
				}
				else
				{
					result.Html = ex.Message;
				}
			}
			catch (Exception ex)
			{
				result.Html = ex.Message;
			}
			return result;
		}

		public static HttpResult Request(string url)
		{
			return Request(new HttpRequest
			{
				Url = url
			});
		}

		public static HttpResult Request(string url, Encoding encoding)
		{
			return Request(new HttpRequest
			{
				Url = url,
				Encoding = encoding
			});
		}

		public static HttpResult Request(string url, System.Net.Http.HttpMethod method, Encoding encoding)
		{
			return Request(new HttpRequest
			{
				Url = url,
				Method = method.ToString(),
				Encoding = encoding
			});
		}

		private static void GetData(HttpRequest item, HttpResult result, HttpWebResponse response)
		{
			//获取StatusCode  
			result.StatusCode = response.StatusCode;
			//获取StatusDescription  
			result.StatusDescription = response.StatusDescription;
			//获取Headers  
			result.Header = response.Headers;
			//获取CookieCollection  
			if (response.Cookies != null)
			{
				result.CookieCollection = response.Cookies;
			}
			//获取set-cookie  
			if (response.Headers["set-cookie"] != null)
			{
				result.Cookie = response.Headers["set-cookie"];
			}

			//处理网页Byte  
			byte[] responseByte = ReadBytes(response);

			if (item.ResultType == ResultType.Byte)
			{
				result.ResultByte = responseByte;
			}
			else
			{
				if (responseByte != null && responseByte.Length > 0)
				{
					if (item.Encoding != null)
					{
						result.Html = item.Encoding.GetString(responseByte);
					}
					else
					{
						//设置编码
						var encoding = EncodingExtensions.GetEncoding(response.CharacterSet, responseByte);
						//得到返回的HTML
						result.Html = encoding.GetString(responseByte);
					}
				}
				else
				{
					//没有返回任何Html代码  
					result.Html = string.Empty;
				}
			}
		}

		private static byte[] ReadBytes(HttpWebResponse response)
		{
			using (var responseSteam = response.GetResponseStream())
			using (var ms = new MemoryStream())
			{
				responseSteam?.CopyTo(ms);
				ms.Seek(0, SeekOrigin.Begin);

				var ms2 = new MemoryStream();

				try
				{
					//GZIIP处理   
					var stream = new GZipStream(ms, CompressionMode.Decompress);
					stream.CopyTo(ms2);
				}
				catch
				{
					ms2 = ms;
				}

				byte[] bytes = ms2.StreamToBytes();

#if NET_CORE
				ms2.Dispose();
#else
				ms2.Close();
#endif
				return bytes;
			}
		}

		private static HttpWebRequest GenerateRequest(HttpRequest item)
		{
			var request = (HttpWebRequest)WebRequest.Create(item.Url);

			SetCer(request, item);

			//设置Header参数  
			if (item.Header != null && item.Header.Count > 0)
			{
				foreach (string key in item.Header.AllKeys)
				{
#if NET_CORE
					request.Headers[key] = item.Header[key];
#else
					request.Headers.Add(key, item.Header[key]);
#endif
				}
			}

			// 设置代理  
			SetProxy(request, item);

			if (item.ProtocolVersion != null)
			{
#if NET_CORE
				request.Headers["Version"] = item.ProtocolVersion.ToString();
#else
				request.ProtocolVersion = item.ProtocolVersion;
#endif
			}

#if !NET_CORE
			request.ServicePoint.Expect100Continue = item.Expect100Continue;
#endif

			//请求方式Get或者Post  
			request.Method = item.Method;
#if NET_CORE
			if (item.KeepAlive)
			{
				request.Headers["Connection"] = "Keep-Alive";
			}
			request.Headers["User-Agent"] = item.UserAgent;
			request.Headers["Referer"] = item.Referer;
#else
			request.Timeout = item.Timeout;
			request.KeepAlive = item.KeepAlive;
			request.ReadWriteTimeout = item.ReadWriteTimeout;
			if (item.IfModifiedSince != null)
			{
				request.IfModifiedSince = Convert.ToDateTime(item.IfModifiedSince);
			}

			request.UserAgent = item.UserAgent;

			request.Referer = item.Referer;

			request.AllowAutoRedirect = item.AllowAutoRedirect;

			if (item.MaximumAutomaticRedirections > 0)
			{
				request.MaximumAutomaticRedirections = item.MaximumAutomaticRedirections;
			}

			//设置最大连接  
			if (item.Connectionlimit > 0)
			{
				request.ServicePoint.ConnectionLimit = item.Connectionlimit;
			}
#endif
			//Accept  
			request.Accept = item.Accept;
			//ContentType返回类型  
			request.ContentType = item.ContentType;

			//设置安全凭证  
			request.Credentials = item.Credentials;
			//设置Cookie  
			SetCookie(request, item);

			//设置Post数据  
			SetPostData(request, item);

			return request;
		}

		private static void SetPostData(HttpWebRequest request, HttpRequest item)
		{
			//验证在得到结果时是否有传入数据  
			var postencoding = Encoding.UTF8;

			if (request.Method.Trim().ToLower().Contains("post"))
			{
				if (item.PostEncoding != null)
				{
					postencoding = item.PostEncoding;
				}
				byte[] buffer = null;
				//写入Byte类型  
				if (item.PostDataType == PostDataType.Byte && item.PostdataByte != null && item.PostdataByte.Length > 0)
				{
					//验证在得到结果时是否有传入数据  
					buffer = item.PostdataByte;
				}
				//写入文件  
				else if (item.PostDataType == PostDataType.File && !string.IsNullOrEmpty(item.Postdata))
				{
					StreamReader r = new StreamReader(File.OpenRead(item.Postdata), postencoding);
					buffer = postencoding.GetBytes(r.ReadToEnd());
#if NET_CORE
					r.Dispose();
#else
					r.Close();
#endif
				}
				//写入字符串  
				else if (!string.IsNullOrEmpty(item.Postdata))
				{
					buffer = postencoding.GetBytes(item.Postdata);
				}
				if (buffer != null)
				{
#if NET_CORE
					request.Headers["Content-Length"] = buffer.Length.ToString();
					request.GetRequestStreamAsync().Result.Write(buffer, 0, buffer.Length);
#else
					request.ContentLength = buffer.Length;
					request.GetRequestStream().Write(buffer, 0, buffer.Length);
#endif
				}
			}
		}

		private static void SetCookie(HttpWebRequest request, HttpRequest item)
		{
			if (!string.IsNullOrEmpty(item.Cookie))
			{
				request.Headers[HttpRequestHeader.Cookie] = item.Cookie;
			}
			//设置CookieCollection
			if (item.ResultCookieType == ResultCookieType.CookieCollection)
			{
				request.CookieContainer = new CookieContainer();
				if (item.CookieCollection != null && item.CookieCollection.Count > 0)
				{
					request.CookieContainer.Add(item.CookieCollection);
				}
			}
		}

		private static void SetProxy(HttpWebRequest request, HttpRequest item)
		{
			if (item.WebProxy != null)
			{
				request.Proxy = item.WebProxy;
			}
		}

		private static void SetCer(HttpWebRequest request, HttpRequest item)
		{
			if (!string.IsNullOrEmpty(item.CerPath))
			{
				//这一句一定要写在创建连接的前面。使用回调的方法进行证书验证。  
				ServicePointManager.ServerCertificateValidationCallback = CheckValidationResult;

				if (item.ClentCertificates != null && item.ClentCertificates.Count > 0)
				{
					foreach (X509Certificate c in item.ClentCertificates)
					{
						request.ClientCertificates.Add(c);
					}
				}

				//将证书添加到请求里  
				request.ClientCertificates.Add(new X509Certificate(item.CerPath));
			}
		}

		/// <summary>  
		/// 回调验证证书问题  
		/// </summary>  
		/// <param name="sender">流对象</param>  
		/// <param name="certificate">证书</param>  
		/// <param name="chain">X509Chain</param>  
		/// <param name="errors">SslPolicyErrors</param>  
		/// <returns>bool</returns>  
		private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) { return true; }
	}

	/// <summary>  
	/// Http请求参考类  
	/// </summary>  
	public class HttpRequest
	{
		public static readonly Version Version10 = new Version("1.0");
		public static readonly Version Version11 = new Version("1.1");

		/// <summary>  
		/// 请求URL必须填写  
		/// </summary>  
		public string Url { get; set; }

		/// <summary>  
		/// 请求方式默认为GET方式,当为POST方式时必须设置Postdata的值  
		/// </summary>  
		public string Method { get; set; } = "GET";

		/// <summary>  
		/// 默认请求超时时间  
		/// </summary>  
		public int Timeout { get; set; } = 100000;

		/// <summary>  
		/// 默认写入Post数据超时间  
		/// </summary>  
		public int ReadWriteTimeout { get; set; } = 30000;

		/// <summary>  
		///  获取或设置一个值，该值指示是否与 Internet 资源建立持久性连接默认为true。  
		/// </summary>  
		public Boolean KeepAlive { get; set; } = true;

		/// <summary>  
		/// 请求标头值 默认为text/html, application/xhtml+xml, */*  
		/// </summary>  
		public string Accept { get; set; } = "text/html, application/xhtml+xml, */*";

		/// <summary>  
		/// 请求返回类型默认 text/html  
		/// </summary>  
		public string ContentType { get; set; } = "text/html";

		/// <summary>  
		/// 客户端访问信息默认Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)  
		/// </summary>  
		public string UserAgent { get; set; } = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)";

		/// <summary>  
		/// 返回数据编码默认为NUll,可以自动识别,一般为utf-8,gbk,gb2312  
		/// </summary>  
		public Encoding Encoding { get; set; }

		/// <summary>  
		/// Post的数据类型  
		/// </summary>  
		public PostDataType PostDataType { get; set; } = PostDataType.String;

		/// <summary>  
		/// Post请求时要发送的字符串Post数据  
		/// </summary>  
		public string Postdata { get; set; } = string.Empty;

		/// <summary>  
		/// Post请求时要发送的Byte类型的Post数据  
		/// </summary>  
		public byte[] PostdataByte { get; set; }

		/// <summary>  
		/// 设置代理对象 
		/// </summary>  
		public IWebProxy WebProxy { get; set; }

		/// <summary>  
		/// Cookie对象集合  
		/// </summary>  
		public CookieCollection CookieCollection { get; set; }

		/// <summary>  
		/// 请求时的Cookie  
		/// </summary>  
		public string Cookie { get; set; }

		/// <summary>  
		/// 来源地址，上次访问地址  
		/// </summary>  
		public string Referer { get; set; } = string.Empty;

		/// <summary>  
		/// 证书绝对路径  
		/// </summary>  
		public string CerPath { get; set; } = string.Empty;

		/// <summary>  
		/// 设置返回类型String和Byte  
		/// </summary>  
		public ResultType ResultType { get; set; } = ResultType.String;

		/// <summary>  
		/// header对象  
		/// </summary>  
		public WebHeaderCollection Header { get; set; } = new WebHeaderCollection();

		/// <summary>  
		// 获取或设置用于请求的 HTTP 版本。返回结果:用于请求的 HTTP 版本。
		/// </summary>  
		public Version ProtocolVersion { get; set; } = Version11;

		/// <summary>  
		///  获取或设置一个 System.Boolean 值，该值确定是否使用 100-Continue 行为。如果 POST 请求需要 100-Continue 响应，则为 true；否则为 false。默认值为 true。  
		/// </summary>  
		public bool Expect100Continue { get; set; } = true;

		/// <summary>  
		/// 设置509证书集合  
		/// </summary>  
		public X509CertificateCollection ClentCertificates { get; set; } = new X509CertificateCollection();

		/// <summary>  
		/// 设置或获取Post参数编码,默认的为Default编码  
		/// </summary>  
		public Encoding PostEncoding { get; set; }

		/// <summary>  
		/// Cookie返回类型,默认的是只返回字符串类型  
		/// </summary>  
		public ResultCookieType ResultCookieType { get; set; } = ResultCookieType.String;

		/// <summary>  
		/// 获取或设置请求的身份验证信息。  
		/// </summary>  
		public ICredentials Credentials { get; set; } = CredentialCache.DefaultCredentials;

		/// <summary>  
		/// 设置请求将跟随的重定向的最大数目  
		/// </summary>  
		public int MaximumAutomaticRedirections { get; set; }

		/// <summary>  
		/// 获取和设置IfModifiedSince，默认为当前日期和时间  
		/// </summary>  
		public DateTime? IfModifiedSince { get; set; }

		public bool AllowAutoRedirect { get; set; } = true;

		public int Connectionlimit { get; set; } = 1024;
	}

	/// <summary>  
	/// Http返回参数类  
	/// </summary>  
	public class HttpResult
	{
		/// <summary>  
		/// Http请求返回的Cookie  
		/// </summary>  
		public string Cookie { get; set; }

		/// <summary>  
		/// Cookie对象集合  
		/// </summary>  
		public CookieCollection CookieCollection { get; set; }

		/// <summary>  
		/// 返回的String类型数据 只有ResultType.String时才返回数据，其它情况为空  
		/// </summary>  
		public string Html { get; set; } = string.Empty;

		/// <summary>  
		/// 返回的Byte数组 只有ResultType.Byte时才返回数据，其它情况为空  
		/// </summary>  
		public byte[] ResultByte { get; set; }

		/// <summary>  
		/// header对象  
		/// </summary>  
		public WebHeaderCollection Header { get; set; }

		/// <summary>  
		/// 返回状态说明  
		/// </summary>  
		public string StatusDescription { get; set; }

		/// <summary>  
		/// 返回状态码,默认为OK  
		/// </summary>  
		public HttpStatusCode StatusCode { get; set; }
	}

	/// <summary>  
	/// Post的数据格式默认为string  
	/// </summary>  
	public enum PostDataType
	{
		/// <summary>  
		/// 字符串类型，这时编码Encoding可不设置  
		/// </summary>  
		String,
		/// <summary>  
		/// Byte类型，需要设置PostdataByte参数的值编码Encoding可设置为空  
		/// </summary>  
		Byte,
		/// <summary>  
		/// 传文件，Postdata必须设置为文件的绝对路径，必须设置Encoding的值  
		/// </summary>  
		File
	}

	/// <summary>  
	/// Cookie返回类型  
	/// </summary>  
	public enum ResultCookieType
	{
		/// <summary>  
		/// 只返回字符串类型的Cookie  
		/// </summary>  
		String,
		/// <summary>  
		/// CookieCollection格式的Cookie集合同时也返回String类型的cookie  
		/// </summary>  
		CookieCollection
	}

	/// <summary>  
	/// 返回类型  
	/// </summary>  
	public enum ResultType
	{
		/// <summary>  
		/// 表示只返回字符串 只有Html有数据  
		/// </summary>  
		String,
		/// <summary>  
		/// 表示返回字符串和字节流 ResultByte和Html都有数据返回  
		/// </summary>  
		Byte
	}
}

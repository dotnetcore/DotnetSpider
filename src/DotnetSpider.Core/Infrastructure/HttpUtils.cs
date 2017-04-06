#if !NET_CORE
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace DotnetSpider.Core.Utils
{
	/// <summary>  
	/// Http连接操作帮助类  
	/// </summary>  
	public class HttpHelper
	{
		#region 预定义方变量
		//默认的编码  
		private Encoding _encoding = Encoding.Default;
		//Post数据编码  
		private Encoding _postencoding = Encoding.Default;
		//HttpWebRequest对象用来发起请求  
		private HttpWebRequest _request;
		//获取影响流的数据对象  
		private HttpWebResponse _response;
		#endregion

		#region Public

		/// <summary>  
		/// 根据相传入的数据，得到相应页面数据  
		/// </summary>  
		/// <param name="item">参数类对象</param>  
		/// <returns>返回HttpResult类型</returns>  
		public HttpResult GetHtml(HttpItem item)
		{
			//返回参数  
			HttpResult result = new HttpResult();
			try
			{
				//准备参数  
				SetRequest(item);
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
				using (_response = (HttpWebResponse)_request.GetResponse())
				{
					GetData(item, result);
				}
			}
			catch (WebException ex)
			{
				if (ex.Response != null)
				{
					using (_response = (HttpWebResponse)ex.Response)
					{
						GetData(item, result);
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
			if (item.IsToLower) result.Html = result.Html.ToLower();
			return result;
		}
		#endregion

		#region GetData

		/// <summary>  
		/// 获取数据的并解析的方法  
		/// </summary>  
		/// <param name="item"></param>  
		/// <param name="result"></param>  
		private void GetData(HttpItem item, HttpResult result)
		{
			#region base
			//获取StatusCode  
			result.StatusCode = _response.StatusCode;
			//获取StatusDescription  
			result.StatusDescription = _response.StatusDescription;
			//获取Headers  
			result.Header = _response.Headers;
			//获取CookieCollection  
			if (_response.Cookies != null) result.CookieCollection = _response.Cookies;
			//获取set-cookie  
			if (_response.Headers["set-cookie"] != null) result.Cookie = _response.Headers["set-cookie"];
			#endregion

			#region byte
			//处理网页Byte  
			byte[] responseByte = GetByte();
			#endregion

			#region Html
			if (responseByte != null && responseByte.Length > 0)
			{
				//设置编码  
				SetEncoding(item, result, responseByte);
				//得到返回的HTML  
				result.Html = _encoding.GetString(responseByte);
			}
			else
			{
				//没有返回任何Html代码  
				result.Html = string.Empty;
			}
			#endregion
		}
		/// <summary>  
		/// 设置编码  
		/// </summary>  
		/// <param name="item">HttpItem</param>  
		/// <param name="result">HttpResult</param>  
		/// <param name="responseByte">byte[]</param>  
		private void SetEncoding(HttpItem item, HttpResult result, byte[] responseByte)
		{
			//是否返回Byte类型数据  
			if (item.ResultType == ResultType.Byte) result.ResultByte = responseByte;
			//从这里开始我们要无视编码了  
			if (_encoding == null)
			{
				Match meta = Regex.Match(Encoding.Default.GetString(responseByte), "<meta[^<]*charset=([^<]*)[\"']", RegexOptions.IgnoreCase);
				string c = string.Empty;
				if (meta != null && meta.Groups.Count > 0)
				{
					c = meta.Groups[1].Value.ToLower().Trim();
				}
				if (c.Length > 2)
				{
					try
					{
						_encoding = Encoding.GetEncoding(c.Replace("\"", string.Empty).Replace("'", "").Replace(";", "").Replace("iso-8859-1", "gbk").Trim());
					}
					catch
					{
						if (string.IsNullOrEmpty(_response.CharacterSet))
						{
							_encoding = Encoding.UTF8;
						}
						else
						{
							_encoding = Encoding.GetEncoding(_response.CharacterSet);
						}
					}
				}
				else
				{
					if (string.IsNullOrEmpty(_response.CharacterSet))
					{
						_encoding = Encoding.UTF8;
					}
					else
					{
						_encoding = Encoding.GetEncoding(_response.CharacterSet);
					}
				}
			}
		}
		/// <summary>  
		/// 提取网页Byte  
		/// </summary>  
		/// <returns></returns>  
		private byte[] GetByte()
		{
			byte[] responseByte = null;
			MemoryStream stream = new MemoryStream();

			//GZIIP处理  
			if (_response.ContentEncoding != null && _response.ContentEncoding.Equals("gzip", StringComparison.InvariantCultureIgnoreCase))
			{
				//开始读取流并设置编码方式  
				stream = GetMemoryStream(new GZipStream(_response.GetResponseStream(), CompressionMode.Decompress));
			}
			else
			{
				//开始读取流并设置编码方式  
				stream = GetMemoryStream(_response.GetResponseStream());
			}
			//获取Byte  
			responseByte = stream.ToArray();
			stream.Close();
			return responseByte;
		}

		/// <summary>  
		/// 4.0以下.net版本取数据使用  
		/// </summary>  
		/// <param name="streamResponse">流</param>  
		private MemoryStream GetMemoryStream(Stream streamResponse)
		{
			MemoryStream stream = new MemoryStream();
			int length = 256;
			Byte[] buffer = new Byte[length];
			int bytesRead = streamResponse.Read(buffer, 0, length);
			while (bytesRead > 0)
			{
				stream.Write(buffer, 0, bytesRead);
				bytesRead = streamResponse.Read(buffer, 0, length);
			}
			return stream;
		}
		#endregion

		#region SetRequest

		/// <summary>  
		/// 为请求准备参数  
		/// </summary>  
		///<param name="item">参数列表</param>  
		private void SetRequest(HttpItem item)
		{
			// 验证证书  
			SetCer(item);
			//设置Header参数  
			if (item.Header != null && item.Header.Count > 0)
				foreach (string key in item.Header.AllKeys)
				{
					_request.Headers.Add(key, item.Header[key]);
				}
			// 设置代理  
			SetProxy(item);
			if (item.ProtocolVersion != null) _request.ProtocolVersion = item.ProtocolVersion;
			_request.ServicePoint.Expect100Continue = item.Expect100Continue;
			//请求方式Get或者Post  
			_request.Method = item.Method;
			_request.Timeout = item.Timeout;
			_request.KeepAlive = item.KeepAlive;
			_request.ReadWriteTimeout = item.ReadWriteTimeout;
			if (item.IfModifiedSince != null) _request.IfModifiedSince = Convert.ToDateTime(item.IfModifiedSince);
			//Accept  
			_request.Accept = item.Accept;
			//ContentType返回类型  
			_request.ContentType = item.ContentType;
			//UserAgent客户端的访问类型，包括浏览器版本和操作系统信息  
			_request.UserAgent = item.UserAgent;
			// 编码  
			_encoding = item.Encoding;
			//设置安全凭证  
			_request.Credentials = item.Credentials;
			//设置Cookie  
			SetCookie(item);
			//来源地址  
			_request.Referer = item.Referer;
			//是否执行跳转功能  
			_request.AllowAutoRedirect = item.Allowautoredirect;
			if (item.MaximumAutomaticRedirections > 0)
			{
				_request.MaximumAutomaticRedirections = item.MaximumAutomaticRedirections;
			}
			//设置Post数据  
			SetPostData(item);
			//设置最大连接  
			if (item.Connectionlimit > 0) _request.ServicePoint.ConnectionLimit = item.Connectionlimit;
		}
		/// <summary>  
		/// 设置证书  
		/// </summary>  
		/// <param name="item"></param>  
		private void SetCer(HttpItem item)
		{
			if (!string.IsNullOrEmpty(item.CerPath))
			{
				//这一句一定要写在创建连接的前面。使用回调的方法进行证书验证。  
				ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
				//初始化对像，并设置请求的URL地址  
				_request = (HttpWebRequest)WebRequest.Create(item.Url);
				SetCerList(item);
				//将证书添加到请求里  
				_request.ClientCertificates.Add(new X509Certificate(item.CerPath));
			}
			else
			{
				//初始化对像，并设置请求的URL地址  
				_request = (HttpWebRequest)WebRequest.Create(item.Url);
				SetCerList(item);
			}
		}
		/// <summary>  
		/// 设置多个证书  
		/// </summary>  
		/// <param name="item"></param>  
		private void SetCerList(HttpItem item)
		{
			if (item.ClentCertificates != null && item.ClentCertificates.Count > 0)
			{
				foreach (X509Certificate c in item.ClentCertificates)
				{
					_request.ClientCertificates.Add(c);
				}
			}
		}
		/// <summary>  
		/// 设置Cookie  
		/// </summary>  
		/// <param name="item">Http参数</param>  
		private void SetCookie(HttpItem item)
		{
			if (!string.IsNullOrEmpty(item.Cookie)) _request.Headers[HttpRequestHeader.Cookie] = item.Cookie;
			//设置CookieCollection  
			if (item.ResultCookieType == ResultCookieType.CookieCollection)
			{
				_request.CookieContainer = new CookieContainer();
				if (item.CookieCollection != null && item.CookieCollection.Count > 0)
					_request.CookieContainer.Add(item.CookieCollection);
			}
		}
		/// <summary>  
		/// 设置Post数据  
		/// </summary>  
		/// <param name="item">Http参数</param>  
		private void SetPostData(HttpItem item)
		{
			//验证在得到结果时是否有传入数据  
			if (!_request.Method.Trim().ToLower().Contains("get"))
			{
				if (item.PostEncoding != null)
				{
					_postencoding = item.PostEncoding;
				}
				byte[] buffer = null;
				//写入Byte类型  
				if (item.PostDataType == PostDataType.Byte && item.PostdataByte != null && item.PostdataByte.Length > 0)
				{
					//验证在得到结果时是否有传入数据  
					buffer = item.PostdataByte;
				}//写入文件  
				else if (item.PostDataType == PostDataType.FilePath && !string.IsNullOrEmpty(item.Postdata))
				{
					StreamReader r = new StreamReader(item.Postdata, _postencoding);
					buffer = _postencoding.GetBytes(r.ReadToEnd());
					r.Close();
				} //写入字符串  
				else if (!string.IsNullOrEmpty(item.Postdata))
				{
					buffer = _postencoding.GetBytes(item.Postdata);
				}
				if (buffer != null)
				{
					_request.ContentLength = buffer.Length;
					_request.GetRequestStream().Write(buffer, 0, buffer.Length);
				}
			}
		}
		/// <summary>  
		/// 设置代理  
		/// </summary>  
		/// <param name="item">参数对象</param>  
		private void SetProxy(HttpItem item)
		{
			bool isIeProxy = false;
			if (!string.IsNullOrEmpty(item.ProxyIp))
			{
				isIeProxy = item.ProxyIp.ToLower().Contains("ieproxy");
			}
			if (!string.IsNullOrEmpty(item.ProxyIp) && !isIeProxy)
			{
				//设置代理服务器  
				if (item.ProxyIp.Contains(":"))
				{
					string[] plist = item.ProxyIp.Split(':');
					WebProxy myProxy = new WebProxy(plist[0].Trim(), Convert.ToInt32(plist[1].Trim()));
					//建议连接  
					myProxy.Credentials = new NetworkCredential(item.ProxyUserName, item.ProxyPwd);
					//给当前请求对象  
					_request.Proxy = myProxy;
				}
				else
				{
					WebProxy myProxy = new WebProxy(item.ProxyIp, false);
					//建议连接  
					myProxy.Credentials = new NetworkCredential(item.ProxyUserName, item.ProxyPwd);
					//给当前请求对象  
					_request.Proxy = myProxy;
				}
			}
			else if (isIeProxy)
			{
				//设置为IE代理  
			}
			else
			{
				_request.Proxy = item.WebProxy;
			}
		}
		#endregion

		#region private main
		/// <summary>  
		/// 回调验证证书问题  
		/// </summary>  
		/// <param name="sender">流对象</param>  
		/// <param name="certificate">证书</param>  
		/// <param name="chain">X509Chain</param>  
		/// <param name="errors">SslPolicyErrors</param>  
		/// <returns>bool</returns>  
		private bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) { return true; }
		#endregion
	}
	/// <summary>  
	/// Http请求参考类  
	/// </summary>  
	public class HttpItem
	{
		string _url = string.Empty;
		/// <summary>  
		/// 请求URL必须填写  
		/// </summary>  
		public string Url
		{
			get { return _url; }
			set { _url = value; }
		}
		string _method = "GET";
		/// <summary>  
		/// 请求方式默认为GET方式,当为POST方式时必须设置Postdata的值  
		/// </summary>  
		public string Method
		{
			get { return _method; }
			set { _method = value; }
		}
		int _timeout = 100000;
		/// <summary>  
		/// 默认请求超时时间  
		/// </summary>  
		public int Timeout
		{
			get { return _timeout; }
			set { _timeout = value; }
		}
		int _readWriteTimeout = 30000;
		/// <summary>  
		/// 默认写入Post数据超时间  
		/// </summary>  
		public int ReadWriteTimeout
		{
			get { return _readWriteTimeout; }
			set { _readWriteTimeout = value; }
		}
		Boolean _keepAlive = true;
		/// <summary>  
		///  获取或设置一个值，该值指示是否与 Internet 资源建立持久性连接默认为true。  
		/// </summary>  
		public Boolean KeepAlive
		{
			get { return _keepAlive; }
			set { _keepAlive = value; }
		}
		string _accept = "text/html, application/xhtml+xml, */*";
		/// <summary>  
		/// 请求标头值 默认为text/html, application/xhtml+xml, */*  
		/// </summary>  
		public string Accept
		{
			get { return _accept; }
			set { _accept = value; }
		}
		string _contentType = "text/html";
		/// <summary>  
		/// 请求返回类型默认 text/html  
		/// </summary>  
		public string ContentType
		{
			get { return _contentType; }
			set { _contentType = value; }
		}
		string _userAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)";
		/// <summary>  
		/// 客户端访问信息默认Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)  
		/// </summary>  
		public string UserAgent
		{
			get { return _userAgent; }
			set { _userAgent = value; }
		}
		Encoding _encoding = null;
		/// <summary>  
		/// 返回数据编码默认为NUll,可以自动识别,一般为utf-8,gbk,gb2312  
		/// </summary>  
		public Encoding Encoding
		{
			get { return _encoding; }
			set { _encoding = value; }
		}
		private PostDataType _postDataType = PostDataType.String;
		/// <summary>  
		/// Post的数据类型  
		/// </summary>  
		public PostDataType PostDataType
		{
			get { return _postDataType; }
			set { _postDataType = value; }
		}
		string _postdata = string.Empty;
		/// <summary>  
		/// Post请求时要发送的字符串Post数据  
		/// </summary>  
		public string Postdata
		{
			get { return _postdata; }
			set { _postdata = value; }
		}
		private byte[] _postdataByte = null;
		/// <summary>  
		/// Post请求时要发送的Byte类型的Post数据  
		/// </summary>  
		public byte[] PostdataByte
		{
			get { return _postdataByte; }
			set { _postdataByte = value; }
		}
		private WebProxy _webProxy;
		/// <summary>  
		/// 设置代理对象，不想使用IE默认配置就设置为Null，而且不要设置ProxyIp  
		/// </summary>  
		public WebProxy WebProxy
		{
			get { return _webProxy; }
			set { _webProxy = value; }
		}

		CookieCollection _cookiecollection = null;
		/// <summary>  
		/// Cookie对象集合  
		/// </summary>  
		public CookieCollection CookieCollection
		{
			get { return _cookiecollection; }
			set { _cookiecollection = value; }
		}
		string _cookie = string.Empty;
		/// <summary>  
		/// 请求时的Cookie  
		/// </summary>  
		public string Cookie
		{
			get { return _cookie; }
			set { _cookie = value; }
		}
		string _referer = string.Empty;
		/// <summary>  
		/// 来源地址，上次访问地址  
		/// </summary>  
		public string Referer
		{
			get { return _referer; }
			set { _referer = value; }
		}
		string _cerPath = string.Empty;
		/// <summary>  
		/// 证书绝对路径  
		/// </summary>  
		public string CerPath
		{
			get { return _cerPath; }
			set { _cerPath = value; }
		}
		private Boolean _isToLower = false;
		/// <summary>  
		/// 是否设置为全文小写，默认为不转化  
		/// </summary>  
		public Boolean IsToLower
		{
			get { return _isToLower; }
			set { _isToLower = value; }
		}
		private Boolean _allowautoredirect = false;
		/// <summary>  
		/// 支持跳转页面，查询结果将是跳转后的页面，默认是不跳转  
		/// </summary>  
		public Boolean Allowautoredirect
		{
			get { return _allowautoredirect; }
			set { _allowautoredirect = value; }
		}
		private int _connectionlimit = 1024;
		/// <summary>  
		/// 最大连接数  
		/// </summary>  
		public int Connectionlimit
		{
			get { return _connectionlimit; }
			set { _connectionlimit = value; }
		}
		private string _proxyusername = string.Empty;
		/// <summary>  
		/// 代理Proxy 服务器用户名  
		/// </summary>  
		public string ProxyUserName
		{
			get { return _proxyusername; }
			set { _proxyusername = value; }
		}
		private string _proxypwd = string.Empty;
		/// <summary>  
		/// 代理 服务器密码  
		/// </summary>  
		public string ProxyPwd
		{
			get { return _proxypwd; }
			set { _proxypwd = value; }
		}
		private string _proxyip = string.Empty;
		/// <summary>  
		/// 代理 服务IP ,如果要使用IE代理就设置为ieproxy  
		/// </summary>  
		public string ProxyIp
		{
			get { return _proxyip; }
			set { _proxyip = value; }
		}
		private ResultType _resulttype = ResultType.String;
		/// <summary>  
		/// 设置返回类型String和Byte  
		/// </summary>  
		public ResultType ResultType
		{
			get { return _resulttype; }
			set { _resulttype = value; }
		}
		private WebHeaderCollection _header = new WebHeaderCollection();
		/// <summary>  
		/// header对象  
		/// </summary>  
		public WebHeaderCollection Header
		{
			get { return _header; }
			set { _header = value; }
		}

		private Version _protocolVersion;

		/// <summary>  
		//     获取或设置用于请求的 HTTP 版本。返回结果:用于请求的 HTTP 版本。默认为 System.Net.HttpVersion.Version11。  
		/// </summary>  
		public Version ProtocolVersion
		{
			get { return _protocolVersion; }
			set { _protocolVersion = value; }
		}
		private Boolean _expect100Continue = true;
		/// <summary>  
		///  获取或设置一个 System.Boolean 值，该值确定是否使用 100-Continue 行为。如果 POST 请求需要 100-Continue 响应，则为 true；否则为 false。默认值为 true。  
		/// </summary>  
		public Boolean Expect100Continue
		{
			get { return _expect100Continue; }
			set { _expect100Continue = value; }
		}
		private X509CertificateCollection _clentCertificates;
		/// <summary>  
		/// 设置509证书集合  
		/// </summary>  
		public X509CertificateCollection ClentCertificates
		{
			get { return _clentCertificates; }
			set { _clentCertificates = value; }
		}
		private Encoding _postEncoding;
		/// <summary>  
		/// 设置或获取Post参数编码,默认的为Default编码  
		/// </summary>  
		public Encoding PostEncoding
		{
			get { return _postEncoding; }
			set { _postEncoding = value; }
		}
		private ResultCookieType _resultCookieType = ResultCookieType.String;
		/// <summary>  
		/// Cookie返回类型,默认的是只返回字符串类型  
		/// </summary>  
		public ResultCookieType ResultCookieType
		{
			get { return _resultCookieType; }
			set { _resultCookieType = value; }
		}

		private ICredentials _iCredentials = CredentialCache.DefaultCredentials;
		/// <summary>  
		/// 获取或设置请求的身份验证信息。  
		/// </summary>  
		public ICredentials Credentials
		{
			get { return _iCredentials; }
			set { _iCredentials = value; }
		}
		/// <summary>  
		/// 设置请求将跟随的重定向的最大数目  
		/// </summary>  
		private int _maximumAutomaticRedirections;

		public int MaximumAutomaticRedirections
		{
			get { return _maximumAutomaticRedirections; }
			set { _maximumAutomaticRedirections = value; }
		}

		private DateTime? _ifModifiedSince = null;
		/// <summary>  
		/// 获取和设置IfModifiedSince，默认为当前日期和时间  
		/// </summary>  
		public DateTime? IfModifiedSince
		{
			get { return _ifModifiedSince; }
			set { _ifModifiedSince = value; }
		}

	}
	/// <summary>  
	/// Http返回参数类  
	/// </summary>  
	public class HttpResult
	{
		private string _cookie;
		/// <summary>  
		/// Http请求返回的Cookie  
		/// </summary>  
		public string Cookie
		{
			get { return _cookie; }
			set { _cookie = value; }
		}

		private CookieCollection _cookieCollection;
		/// <summary>  
		/// Cookie对象集合  
		/// </summary>  
		public CookieCollection CookieCollection
		{
			get { return _cookieCollection; }
			set { _cookieCollection = value; }
		}
		private string _html = string.Empty;
		/// <summary>  
		/// 返回的String类型数据 只有ResultType.String时才返回数据，其它情况为空  
		/// </summary>  
		public string Html
		{
			get { return _html; }
			set { _html = value; }
		}
		private byte[] _resultByte;
		/// <summary>  
		/// 返回的Byte数组 只有ResultType.Byte时才返回数据，其它情况为空  
		/// </summary>  
		public byte[] ResultByte
		{
			get { return _resultByte; }
			set { _resultByte = value; }
		}
		private WebHeaderCollection _header;
		/// <summary>  
		/// header对象  
		/// </summary>  
		public WebHeaderCollection Header
		{
			get { return _header; }
			set { _header = value; }
		}
		private string _statusDescription;
		/// <summary>  
		/// 返回状态说明  
		/// </summary>  
		public string StatusDescription
		{
			get { return _statusDescription; }
			set { _statusDescription = value; }
		}
		private HttpStatusCode _statusCode;
		/// <summary>  
		/// 返回状态码,默认为OK  
		/// </summary>  
		public HttpStatusCode StatusCode
		{
			get { return _statusCode; }
			set { _statusCode = value; }
		}
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
		FilePath
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
}
#endif
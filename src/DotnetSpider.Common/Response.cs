using Newtonsoft.Json;
using System.Net;

namespace DotnetSpider.Common
{
	/// <summary>
	/// 链接请求结果
	/// </summary>
	public class Response
	{
		public Response()
		{
		}

		public Response(Request request)
		{
			Request = request;
		}

		/// <summary>
		/// 链接请求
		/// </summary>
		public Request Request { get; set; }

		/// <summary>
		/// 最终请求的链接, 当发生302跳转时可能与请求的Url不一致
		/// </summary>
		public string TargetUrl { get; set; }

		/// <summary>
		/// 请求的结果
		/// </summary>
		public string Content { get; set; }

		/// <summary>
		/// 请求结果的类型
		/// </summary>
		public ContentType ContentType { get; set; }

		/// <summary>
		/// 用于数据传递
		/// </summary>
		public dynamic Delivery { get; set; }

		[JsonIgnore]
		public HttpStatusCode StatusCode { get; set; }
	}
}
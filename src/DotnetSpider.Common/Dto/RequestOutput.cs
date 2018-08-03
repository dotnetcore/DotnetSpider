using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DotnetSpider.Common;

namespace DotnetSpider.Common.Dto
{
	/// <summary>
	/// 链接请求
	/// </summary>
	public class RequestOutput
	{
		/// <summary>
		/// 当前链接的深度, 默认构造的链接深度为1, 用于控制爬取的深度
		/// </summary>
		public int Depth { get; set; }

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
		/// 请求此链接时需要POST的数据
		/// </summary>
		public string Content { get; set; }

		/// <summary>
		/// 请求链接, 请求链接限定为Uri的原因: 无论是本地文件资源或者网络资源都是可以用Uri来定义的
		/// 比如本地文件: file:///C:/Users/Lewis/Desktop/111.png
		/// </summary>
		public string Url { get; set; }

		/// <summary>
		/// 请求标识
		/// </summary>
		public virtual string Identity => $"{Referer}.{Origin}.{Method}.{Content}.{Url}".ToShortMd5();

		public Request ToRequest()
		{
			return new Request(Url) { Depth = Depth, Content = Content, Method = Method, Origin = Origin, Referer = Referer };
		}
	}
}

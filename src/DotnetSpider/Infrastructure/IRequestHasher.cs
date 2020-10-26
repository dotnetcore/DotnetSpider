using System;
using System.Collections.Generic;
using System.Text;
using DotnetSpider.Http;

namespace DotnetSpider.Infrastructure
{
	/// <summary>
	/// 请求哈希器接口
	/// </summary>
	public interface IRequestHasher
	{
		/// <summary>
		/// 编译Hash
		/// </summary>
		/// <returns></returns>
		string ComputeHash(Request request);
	}
}

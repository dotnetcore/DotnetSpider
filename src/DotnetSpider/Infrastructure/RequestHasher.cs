using System;
using System.Collections.Generic;
using System.Text;
using DotnetSpider.Extensions;
using DotnetSpider.Http;

namespace DotnetSpider.Infrastructure
{
	/// <summary>
	///请求哈希编译器
	/// </summary>
	public class RequestHasher : IRequestHasher
	{
		private readonly IHashAlgorithmService _hashAlgorithmService;

		public RequestHasher(IHashAlgorithmService hashAlgorithmService)
		{
			_hashAlgorithmService = hashAlgorithmService;
		}

		public string ComputeHash(Request request)
		{
			var bytes = new
			{
				request.Owner,
				request.RequestUri.AbsoluteUri,
				request.Method,
				request.RequestedTimes,
				request.Content
			}.Serialize();
			return _hashAlgorithmService.ComputeHash(bytes).ToBase64String();
		}
	}
}

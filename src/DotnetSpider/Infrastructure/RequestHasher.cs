using System;
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

		public void ComputeHash(Request request)
		{
			var bytes = new
			{
				request.Owner,
				request.RequestUri.AbsoluteUri,
				request.Method,
				request.RequestedTimes,
				request.Content
			}.Serialize();
			request.Hash = Convert.ToBase64String(_hashAlgorithmService.ComputeHash(bytes)).TrimEnd('=');
			// return request.Hash;
		}
	}
}

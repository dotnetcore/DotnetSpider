using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;

namespace DotnetSpider.Portal.Common
{
	public static class HttpClientFactory
	{
		private static readonly Dictionary<string, HttpClient> _httpClients = new();

		[MethodImpl(MethodImplOptions.Synchronized)]
		public static HttpClient GetHttpClient(string name, string user, string password)
		{
			name = string.IsNullOrWhiteSpace(name) ? "default" : name;
			if (!_httpClients.ContainsKey(name))
			{
				var httpClient = new HttpClient(new SocketsHttpHandler
				{
					Credentials = string.IsNullOrWhiteSpace(user) ? null : new NetworkCredential(user, password)
				});
				_httpClients.Add(name, httpClient);
			}

			return _httpClients[name];
		}
	}
}
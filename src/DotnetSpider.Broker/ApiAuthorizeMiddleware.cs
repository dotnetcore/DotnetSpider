using Microsoft.AspNetCore.Http;
using System.Net;
using System.Threading.Tasks;

namespace DotnetSpider.Broker
{
	/// <summary>
	/// 仅仅作用于外部请求通信时使用
	/// </summary>
	public class ApiAuthorizeMiddleware
	{
		private readonly RequestDelegate _delegate;
		private readonly BrokerOptions _options;

		public ApiAuthorizeMiddleware(RequestDelegate @delegate, BrokerOptions options)
		{
			_delegate = @delegate;
			_options = options;
		}

		public async Task Invoke(HttpContext context)
		{
			if (!IsAuth(context))
			{
				context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
				return;
			}
			await _delegate.Invoke(context);
		}

		protected bool IsAuth(HttpContext context)
		{
			if (!_options.UseToken)
			{
				return true;
			}
			if (context.Request.Headers.ContainsKey(BrokerOptions.TokenHeader))
			{
				var token = context.Request.Headers[BrokerOptions.TokenHeader].ToString();
				return _options.Tokens.Contains(token);
			}
			else
			{
				return false;
			}
		}
	}
}

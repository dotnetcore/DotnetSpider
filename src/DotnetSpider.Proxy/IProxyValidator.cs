using System.Net;

namespace DotnetSpider.Proxy
{
	public interface IProxyValidator
	{
		bool IsAvailable(WebProxy proxy);
	}
}

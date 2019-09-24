using System.Net;
using System.Threading.Tasks;

namespace DotnetSpider.Downloader
{
	/// <summary>
	/// 代理验证器
	/// </summary>
	public interface IProxyValidator
	{
		/// <summary>
		/// 判断代理是否可用
		/// </summary>
		/// <param name="proxy">代理</param>
		/// <returns></returns>
		Task<bool> IsAvailable(WebProxy proxy);
	}
}

using System.Threading.Tasks;

namespace DotnetSpider.Core
{
	/// <summary>
	/// 运行接口定义
	/// </summary>
	public interface IRunable
	{
		/// <summary>
		/// 异步运行程序
		/// </summary>
		/// <param name="arguments">程序运行的参数</param>
		/// <returns></returns>
		Task RunAsync(params string[] arguments);

		/// <summary>
		/// 运行程序
		/// </summary>
		/// <param name="arguments">程序运行的参数</param>
		void Run(params string[] arguments);
	}
}

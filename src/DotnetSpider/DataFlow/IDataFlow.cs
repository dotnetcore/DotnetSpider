using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.DataFlow
{
	/// <summary>
	/// 数据流处理器
	/// </summary>
	public interface IDataFlow : IDisposable
	{
		/// <summary>
		/// 日志接口
		/// </summary>
		ILogger Logger { get; set; }

		/// <summary>
		/// 名称
		/// </summary>
		string Name { get; }

		/// <summary>
		/// 初始化
		/// </summary>
		/// <returns></returns>
		Task InitAsync();

		/// <summary>
		/// 流处理
		/// </summary>
		/// <param name="context">处理上下文</param>
		/// <returns></returns>
		Task<DataFlowResult> HandleAsync(DataFlowContext context);
	}
}
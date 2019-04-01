using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.Data
{
	/// <summary>
	/// 数据流处理器基类
	/// </summary>
	public abstract class DataFlowBase : IDataFlow
	{
		/// <summary>
		/// 日志接口
		/// </summary>
		public ILogger Logger { get; set; }

		/// <summary>
		/// 初始化
		/// </summary>
		/// <returns></returns>
		public virtual Task InitAsync()
		{
#if NETFRAMEWORK
            return DotnetSpider.Core.Framework.CompletedTask;
#else
			return Task.CompletedTask;
#endif
		}

		/// <summary>
		/// 流处理
		/// </summary>
		/// <param name="context">处理上下文</param>
		/// <returns></returns>
		public abstract Task<DataFlowResult> HandleAsync(DataFlowContext context);

		/// <summary>
		/// 释放
		/// </summary>
		public virtual void Dispose()
		{
		}
	}
}
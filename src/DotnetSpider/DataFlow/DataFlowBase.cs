using System.Threading.Tasks;
using DotnetSpider.Infrastructure;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.DataFlow
{
	/// <summary>
	/// 数据流处理器基类
	/// </summary>
	public abstract class DataFlowBase : IDataFlow
	{
		protected ILogger Logger { get; private set; }

		/// <summary>
		/// 初始化
		/// </summary>
		/// <returns></returns>
		public abstract Task InitializeAsync();

		public void SetLogger(ILogger logger)
		{
			logger.NotNull(nameof(logger));
			Logger = logger;
		}

		/// <summary>
		/// 流处理
		/// </summary>
		/// <param name="context">处理上下文</param>
		/// <returns></returns>
		public abstract Task HandleAsync(DataFlowContext context);

		/// <summary>
		/// 是否为空
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		protected virtual bool IsNullOrEmpty(DataFlowContext context)
		{
			return context.IsEmpty;
		}

		/// <summary>
		/// 释放
		/// </summary>
		public virtual void Dispose()
		{
		}
	}
}

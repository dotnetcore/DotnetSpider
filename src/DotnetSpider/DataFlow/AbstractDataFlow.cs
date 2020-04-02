using System.Threading.Tasks;
using DotnetSpider.Infrastructure;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.DataFlow
{
    /// <summary>
    /// 数据流处理器基类
    /// </summary>
    public abstract class AbstractDataFlow : IDataFlow
    {
        protected ILogger Logger { get; private set; }

        public virtual string Name => GetType().Name;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public virtual Task InitAsync()
        {
            return Task.CompletedTask;
        }

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
        public abstract Task HandleAsync(DataContext context);

        /// <summary>
        /// 释放
        /// </summary>
        public virtual void Dispose()
        {
        }
    }
}
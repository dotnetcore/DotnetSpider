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
        /// 初始化
        /// </summary>
        /// <returns></returns>
        Task InitializeAsync();

        /// <summary>
        /// 设置日志
        /// </summary>
        /// <param name="logger"></param>
        void SetLogger(ILogger logger);

        /// <summary>
        /// 流处理
        /// </summary>
        /// <param name="context">处理上下文</param>
        /// <returns></returns>
        Task HandleAsync(DataFlowContext context);
    }
}

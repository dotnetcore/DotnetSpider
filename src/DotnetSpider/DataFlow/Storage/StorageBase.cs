using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.DataFlow.Storage
{
    /// <summary>
    /// 存储器
    /// </summary>
    public abstract class StorageBase : DataFlowBase
    {
        public override async Task HandleAsync(DataFlowContext context)
        {
            if (context.IsEmpty)
            {
                Logger.LogWarning("数据流上下文不包含实体解析结果");
                return;
            }

            await StoreAsync(context);
        }

        protected abstract Task StoreAsync(DataFlowContext context);
    }
}

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.DataFlow.Storage
{
    /// <summary>
    /// 存储器
    /// </summary>
    public abstract class StorageBase : AbstractDataFlow
    {
        public override async Task HandleAsync(DataContext context)
        {
            if (context.IsEmpty)
            {
                Logger.LogWarning("数据流上下文不包含实体解析结果");
                return;
            }

            await StoreAsync(context);
        }

        protected abstract Task StoreAsync(DataContext context);
    }
}
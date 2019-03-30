using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.Data.Storage
{
    public abstract class EntityStorageBase : StorageBase
    {
        public override async Task<DataFlowResult> HandleAsync(DataFlowContext context)
        {
            try
            {
                if (!context.HasParseItems)
                {
                    Logger.LogWarning("数据流上下文不包含实体解析结果");
                    return DataFlowResult.Success;
                }

                var storeResult = await Store(context);
                if (storeResult == DataFlowResult.Failed || storeResult == DataFlowResult.Terminated)
                {
                    return storeResult;
                }

                return DataFlowResult.Success;
            }
            catch (Exception e)
            {
                Logger?.LogError($"数据存储发生异常: {e}");
                return DataFlowResult.Failed;
            }
        }
    }
}
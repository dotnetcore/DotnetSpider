using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.DataFlow.Storage
{
	/// <summary>
	/// 实体存储器
	/// </summary>
	public abstract class EntityStorageBase : StorageBase
	{
		public override async Task<DataFlowResult> HandleAsync(DataFlowContext context)
		{
			try
			{
				if (!context.HasParseData)
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
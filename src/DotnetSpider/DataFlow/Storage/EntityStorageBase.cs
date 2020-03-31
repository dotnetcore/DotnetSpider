using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.DataFlow.Storage
{
    /// <summary>
    /// 实体存储器
    /// </summary>
    public abstract class EntityStorageBase : AbstractDataFlow
    {
        private readonly Type _baseType = typeof(IEntity);
        protected abstract Task StoreAsync(DataContext context, Dictionary<Type, List<dynamic>> dict);

        public override async Task HandleAsync(DataContext context)
        {
            if (context.IsEmpty)
            {
                Logger.LogWarning("数据流上下文不包含实体解析结果");
                return;
            }

            var data = context.GetData();
            var dict = new Dictionary<Type, List<dynamic>>();
            foreach (var d in data)
            {
                if (d.Key is Type type && d.Value is IList list && list.Count > 0 &&
                    _baseType.IsInstanceOfType(list[0]))
                {
                    if (!dict.ContainsKey(type))
                    {
                        dict.Add(type, new List<dynamic>());
                    }

                    foreach (var entity in d.Value)
                    {
                        dict[type].Add(entity);
                    }
                }
            }

            await StoreAsync(context, dict);
        }
    }
}
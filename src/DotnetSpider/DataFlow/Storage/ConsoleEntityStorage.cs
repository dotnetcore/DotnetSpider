using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DotnetSpider.DataFlow.Storage
{
    /// <summary>
    /// 控制台打印(实体)解析结果
    /// </summary>
    public class ConsoleEntityStorage : EntityStorageBase
    {
        public static IDataFlow CreateFromOptions(SpiderOptions options)
        {
            return new ConsoleEntityStorage();
        }
        
        protected override Task StoreAsync(DataContext context, Dictionary<Type, List<dynamic>> dict)
        {
            foreach (var kv in dict)
            {
                Console.WriteLine(JsonConvert.SerializeObject(kv.Value));
            }

            return Task.CompletedTask;
        }
    }
}
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DotnetSpider.DataFlow.Storage
{
    /// <summary>
    /// 控制台打印解析结果(所有解析结果)
    /// </summary>
    public class ConsoleStorage : StorageBase
    {
        public static IDataFlow CreateFromOptions(SpiderOptions options)
        {
            return new ConsoleStorage();
        }
        
        protected override Task StoreAsync(DataContext context)
        {
            var items = context.GetData();
            Console.WriteLine(JsonConvert.SerializeObject(items));
            return Task.CompletedTask;
        }
    }
}
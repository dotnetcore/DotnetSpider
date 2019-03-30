using System;
using System.Threading.Tasks;
using DotnetSpider.Core;
using Newtonsoft.Json;

namespace DotnetSpider.Data.Storage
{
    public class ConsoleStorage : StorageBase
    {
        public static ConsoleStorage CreateFromOptions(ISpiderOptions options)
        {
            return new ConsoleStorage();
        }
        
        protected override Task<DataFlowResult> Store(DataFlowContext context)
        {
            var items = context.GetItems();
            Console.WriteLine(JsonConvert.SerializeObject(items));
            return Task.FromResult(DataFlowResult.Success);
        }
    }
}
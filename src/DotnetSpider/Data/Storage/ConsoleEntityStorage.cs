using System;
using System.Threading.Tasks;
using DotnetSpider.Core;
using Newtonsoft.Json;

namespace DotnetSpider.Data.Storage
{
    public class ConsoleEntityStorage : StorageBase
    {
        public static ConsoleEntityStorage CreateFromOptions(ISpiderOptions options)
        {
            return new ConsoleEntityStorage();
        }
        
        protected override Task<DataFlowResult> Store(DataFlowContext context)
        {
            var items = context.GetItems();
            foreach (var item in items)
            {
                foreach (var data in item.Value)
                {
                    Console.WriteLine(JsonConvert.SerializeObject(data));
                }
            }

            return Task.FromResult(DataFlowResult.Success);
        }
    }
}
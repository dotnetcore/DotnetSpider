using System.IO;
using System.Threading.Tasks;
using DotnetSpider.Core;
using Newtonsoft.Json;

namespace DotnetSpider.Data.Storage
{
    public class JsonFileStorage : FileStorageBase
    {
        public static JsonFileStorage CreateFromOptions(ISpiderOptions options)
        {
            return new JsonFileStorage();
        }
        
        protected override async Task<DataFlowResult> Store(DataFlowContext context)
        {
            var items = context.GetItems();
            var file = Path.Combine(GetDataFolder(context.Response.Request.OwnerId), $"{context.Response.Request.Hash}.json");
            CreateFile(file);
            await Writer.WriteLineAsync(JsonConvert.SerializeObject(items));

            return DataFlowResult.Success;
        }
    }
}
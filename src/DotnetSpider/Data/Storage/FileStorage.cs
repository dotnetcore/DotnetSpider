using System.IO;
using System.Threading.Tasks;
using DotnetSpider.Core;
using Newtonsoft.Json;

namespace DotnetSpider.Data.Storage
{
    public class FileStorage : FileStorageBase
    {
        public static FileStorage CreateFromOptions(ISpiderOptions options)
        {
            return new FileStorage();
        }
        
        protected override async Task<DataFlowResult> Store(DataFlowContext context)
        {
            var file = Path.Combine(GetDataFolder(context.Response.Request.OwnerId), $"{context.Response.Request.Hash}.data");
            CreateFile(file);

            await Writer.WriteLineAsync("URL:\t" + context.Response.Request.Url);
            var items = context.GetItems();
            await Writer.WriteLineAsync("DATA:\t" + JsonConvert.SerializeObject(items));

            return DataFlowResult.Success;
        }
    }
}
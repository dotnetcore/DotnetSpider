using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DotnetSpider.DataFlow.Storage
{
    /// <summary>
    /// JSON 文件保存解析结果(所有解析结果)
    /// 保存路径: [当前程序运行目录]/files/[任务标识]/[request.hash].json
    /// </summary>
    public class JsonFileStorage : FileStorageBase
    {
        public static IDataFlow CreateFromOptions(SpiderOptions options)
        {
            return new JsonFileStorage();
        }

        protected override async Task StoreAsync(DataContext context)
        {
            var file = Path.Combine(GetDataFolder(context.Request.Owner),
                $"{context.Request.Hash}.json");
            using var writer = OpenWrite(file);
            var items = context.GetData().Where(x => !ReferenceEquals(x.Key, Consts.ResponseBytes));
            await writer.WriteLineAsync(JsonConvert.SerializeObject(items));
        }
    }
}

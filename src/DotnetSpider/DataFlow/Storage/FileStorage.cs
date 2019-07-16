using System.IO;
using System.Threading.Tasks;
using DotnetSpider.Common;
using Newtonsoft.Json;

namespace DotnetSpider.DataFlow.Storage
{
	/// <summary>
	/// 文件保存解析结果(所有解析结果)
	/// 保存路径: [当前程序运行目录]/files/[任务标识]/[request.hash].data
	/// </summary>
    public class FileStorage : FileStorageBase
    {
	    /// <summary>
	    /// 根据配置返回存储器
	    /// </summary>
	    /// <param name="options">配置</param>
	    /// <returns></returns>
        public static FileStorage CreateFromOptions(SpiderOptions options)
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
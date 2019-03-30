using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DotnetSpider.Core;
using DotnetSpider.Data.Storage.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DotnetSpider.Data.Storage
{
    public class JsonEntityFileStorage : EntityStorageBase
    {
        private readonly ConcurrentDictionary<string, StreamWriter> _writers =
            new ConcurrentDictionary<string, StreamWriter>();

        public static JsonEntityFileStorage CreateFromOptions(ISpiderOptions options)
        {
            return new JsonEntityFileStorage();
        }
        
        protected override async Task<DataFlowResult> Store(DataFlowContext context)
        {
            foreach (var item in context.GetParseItems())
            {
                var tableMetadata = (TableMetadata) context[item.Key];
                var file = Path.Combine(Framework.BaseDirectory, "json",
                    $"{GenerateFileName(tableMetadata)}.json");

                StreamWriter writer = CreateOrOpen(file);
                await writer.WriteLineAsync(JsonConvert.SerializeObject(item.Value));
            }

            return DataFlowResult.Success;
        }

        public override void Dispose()
        {
            base.Dispose();

            foreach (var writer in _writers)
            {
                try
                {
                    writer.Value.Dispose();
                }
                catch (Exception e)
                {
                    Logger?.LogError($"释放 JSON 文件 {writer.Key} 失败: {e}");
                }
            }
        }

        private string GenerateFileName(TableMetadata tableMetadata)
        {
            return string.IsNullOrWhiteSpace(tableMetadata.Schema.Database)
                ? $"{tableMetadata.Schema.Table}"
                : $"{tableMetadata.Schema.Database}.{tableMetadata.Schema.Table}";
        }

        private StreamWriter CreateOrOpen(string file)
        {
            return _writers.GetOrAdd(file, x =>
            {
                var folder = Path.GetDirectoryName(x);
                if (!string.IsNullOrWhiteSpace(folder) && !Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                return new StreamWriter(File.OpenWrite(x), Encoding.UTF8);
            });
        }
    }
}
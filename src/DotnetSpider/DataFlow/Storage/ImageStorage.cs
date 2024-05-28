using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotnetSpider.DataFlow.Storage;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace DotnetSpider.DataFlow;

public class ImageStorage : FileStorageBase
{
    private HashSet<string> _imageExtensions;
    private static readonly object Locker = new();

    protected override bool IsNullOrEmpty(DataFlowContext context)
    {
        return context.Response == null || context.Response.Content.Bytes.Length == 0;
    }

    public string[] ImageExtensions { get; set; } = { "jpeg", "gif", "jpg", "bmp", "png", "ico", "svg" };

    public override Task InitializeAsync()
    {
        base.InitializeAsync();

        _imageExtensions = new HashSet<string>(ImageExtensions);
        return Task.CompletedTask;
    }

    public override async Task HandleAsync(DataFlowContext context, ResponseDelegate next)
    {
        if (IsNullOrEmpty(context))
        {
            Logger.LogWarning("数据流上下文不包含解析结果");
        }
        else
        {
            var fileName = context.Request.RequestUri.AbsolutePath;
            if (_imageExtensions.Any(x => fileName.EndsWith(x)))
            {
                var path = Path.Combine(GetDataFolder(context.Request.Owner), "images");
                path = $"{path}{fileName}";
                var folder = Path.GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(folder))
                {
                    lock (Locker)
                    {
                        if (!Directory.Exists(folder))
                        {
                            Directory.CreateDirectory(folder);
                        }
                    }

                    await File.WriteAllBytesAsync(path, context.Response.Content.Bytes);
                }
            }
        }

        await next(context);
    }
}

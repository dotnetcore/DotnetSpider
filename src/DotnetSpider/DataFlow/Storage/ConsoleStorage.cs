using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace DotnetSpider.DataFlow;

/// <summary>
/// 控制台打印解析结果(所有解析结果)
/// </summary>
public class ConsoleStorage : DataFlowBase
{
    public static IDataFlow CreateFromOptions(IConfiguration _)
    {
        return new ConsoleStorage();
    }

    public override Task InitializeAsync()
    {
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
            var data = context.Data;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(
                $"{Environment.NewLine}DATA: {System.Text.Json.JsonSerializer.Serialize(data)}");
        }

        await next(context);
    }
}

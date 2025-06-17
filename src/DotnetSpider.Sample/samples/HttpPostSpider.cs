using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.DataFlow;
using DotnetSpider.DataFlow.Parser;
using DotnetSpider.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace DotnetSpider.Sample.samples;
/// <summary>
/// HttpPost
/// </summary>
[DisplayName("HttpPost")]
public class HttpPostSpider(
    IOptions<SpiderOptions> options,
    DependenceServices services,
    ILogger<Spider> logger)
    : Spider(options, services, logger)
{
    public static async Task RunAsync()
    {
        var builder = Builder.CreateDefaultBuilder<HttpPostSpider>(x =>
        {
            x.Speed = 1;
        });
        builder.UseSerilog();
        await builder.Build().RunAsync();
    }

    protected override async Task InitializeAsync(CancellationToken stoppingToken = default)
    {
        AddDataFlow<HttpPostParser>();
        AddDataFlow(GetDefaultStorage);

        var request = new Request("https://www.baidu.com");
        request.Method = "POST";
        // 原始表单数据
        var formData = $"a=a";
        // 添加表单数据
        var formContent = new ByteArrayContent(Encoding.UTF8.GetBytes(formData));
        formContent.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
        request.Content = formContent;

        await AddRequestsAsync(request);
    }

    protected class HttpPostParser : DataParser
    {
        public override Task InitializeAsync()
        {

            return Task.CompletedTask;
        }

        protected override Task ParseAsync(DataFlowContext context)
        {
            var selectValue = context.Selectable.Value;

            //var resutl = JsonConvert.DeserializeObject<ResponseResult<T>>(selectValue);
            //var list = resutl?.Data;
            ////插入数据库
            //var typeName = typeof(DistrictEntity);
            //context.AddData(typeName, list);

            return Task.CompletedTask;
        }
    }
}

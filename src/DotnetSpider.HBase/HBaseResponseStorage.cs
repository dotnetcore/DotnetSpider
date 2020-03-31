using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DotnetSpider.DataFlow;
using DotnetSpider.DataFlow.Storage;
using DotnetSpider.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.HBase
{
    /// <summary>
    /// Can't create namespace dotnet_spider by REST
    /// So pls create it by HBase shell
    /// </summary>
    public class HBaseResponseStorage : StorageBase
    {
        private static readonly object Locker = new object();
        private readonly string _rest;
        private string _table;
        private readonly string _columnName = "data:".ToBase64String();

        /// <summary>
        /// 根据配置返回存储器
        /// </summary>
        /// <param name="options">配置</param>
        /// <returns></returns>
        public static HBaseResponseStorage CreateFromOptions(SpiderOptions options)
        {
            var storage = new HBaseResponseStorage(options.HBaseRestServer);
            return storage;
        }

        public HBaseResponseStorage(string restServer)
        {
            var uri = new Uri(restServer);
            _rest = uri.ToString();
        }

        protected override async Task StoreAsync(DataContext context)
        {
            var id = context.Request.Owner;
            var table = $"dotnet_spider:response_{id}";
            var httpClient = context.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(_rest);
            lock (Locker)
            {
                EnsureDatabaseAndTableCreated(httpClient, table);
            }

            var hash = context.Request.Hash;
            var data = Convert.ToBase64String((byte[]) context[Consts.ResponseBytes]);

            for (var i = 0; i < 3; ++i)
            {
                try
                {
                    var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, $"{_rest}{table}/row");
                    httpRequestMessage.Headers.TryAddWithoutValidation("Accept", "application/json");
                    var rowKey = hash.ToBase64String();

                    var body =
                        "{\"Row\":[{\"key\":\"" + rowKey +
                        "\", \"Cell\": [{\"column\":\"" + _columnName + "\", \"$\":\"" + data + "\"}]}]}";
                    var content =
                        new StringContent(body,
                            Encoding.UTF8, "application/json");
                    httpRequestMessage.Content = content;

                    var res = await httpClient.SendAsync(httpRequestMessage);
                    res.EnsureSuccessStatusCode();
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Store {context.Request.RequestUri} response to HBase failed [{i}]: {ex}");
                }
            }
        }

        private void EnsureDatabaseAndTableCreated(HttpClient httpClient, string table)
        {
            if (string.IsNullOrWhiteSpace(_table))
            {
                Logger.LogInformation($"Create table: {table}");
                var body =
                    $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><TableSchema name=\"{table}\"><ColumnSchema name=\"data\"/></TableSchema>";
                var createTableUrl = $"{_rest}{table}/schema";
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, createTableUrl);
                httpRequestMessage.Headers.TryAddWithoutValidation("Accept", "text/xml");

                var content = new StringContent(body, Encoding.UTF8, "text/xml");
                httpRequestMessage.Content = content;

                var res = httpClient.SendAsync(httpRequestMessage).GetAwaiter().GetResult();
                res.EnsureSuccessStatusCode();
                _table = table;
            }
        }
    }
}
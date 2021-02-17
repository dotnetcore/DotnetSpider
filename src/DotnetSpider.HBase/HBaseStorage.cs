using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DotnetSpider.DataFlow;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.HBase
{
	public class HBaseOptions
	{
		private readonly IConfiguration _configuration;

		public HBaseOptions(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public string HBaseRestServer => _configuration["HBase:RestServer"];
	}

	/// <summary>
	/// Can't create namespace dotnet_spider by REST
	/// So pls create it by HBase shell:
	/// $ hbase shell
	/// $ create_namespace 'dotnet_spider'
	/// </summary>
	public class HBaseStorage : DataFlowBase
	{
		private readonly string _rest;

		private readonly string _columnName = Convert.ToBase64String(Encoding.UTF8.GetBytes("data:"));

		private readonly ConcurrentDictionary<string, bool>
			_tableCreatedDict = new();

		/// <summary>
		/// 根据配置返回存储器
		/// </summary>
		/// <param name="configuration">配置</param>
		/// <returns></returns>
		public static HBaseStorage CreateFromOptions(IConfiguration configuration)
		{
			var options = new HBaseOptions(configuration);
			var storage = new HBaseStorage(options.HBaseRestServer);
			return storage;
		}

		public override Task InitializeAsync()
		{
			return Task.CompletedTask;
		}

		public HBaseStorage(string restServer)
		{
			var uri = new Uri(restServer);
			_rest = uri.ToString();
		}

		public override async Task HandleAsync(DataFlowContext context)
		{
			var id = context.Request.Owner;
			var table = $"dotnet_spider:response_{id}";
			_tableCreatedDict.GetOrAdd(table, t =>
			{
				EnsureDatabaseAndTableCreated(context, t);
				return true;
			});

			var hash = context.Request.Hash;

			var bytes = context.MessageBytes;
			var data = Convert.ToBase64String(bytes);

			var httpClient = context.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(_rest);
			for (var i = 0; i < 3; ++i)
			{
				try
				{
					var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, $"{_rest}{table}/row");
					httpRequestMessage.Headers.TryAddWithoutValidation("Accept", "application/json");
					var rowKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(hash));

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

		private void EnsureDatabaseAndTableCreated(DataFlowContext context, string table)
		{
			var httpClient = context.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(_rest);
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
		}
	}
}

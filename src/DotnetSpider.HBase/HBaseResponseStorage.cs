using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DotnetSpider.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


// ReSharper disable once CheckNamespace
namespace DotnetSpider.DataFlow.Storage
{
	public class HBaseResponseStorage : DataFlowBase
	{
		private readonly HttpClient _httpClient;
		private readonly string _url;
		private readonly string _createTableUrl;

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
			_url = $"{uri}dotnetspider:spider_response/row";
			_createTableUrl = $"{uri}dotnetspider:spider_response/schema";
			_httpClient = new HttpClient();
		}

		public override async Task<DataFlowResult> HandleAsync(DataFlowContext context)
		{
			var hash = context.Response.Request.Hash;
			var response = JsonConvert.SerializeObject(context.Response).ToBase64String();

			var body = JsonConvert.SerializeObject(new
			{
				Row = new List<Row>
				{
					new Row
					{
						Key = hash.ToBase64String(),
						Cell = new List<Cell>
						{
							new Cell {Column = "response:content".ToBase64String(), Value = response}
						}
					}
				}
			});
			for (int i = 0; i < 10; ++i)
			{
				try
				{
					var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, _url);
					httpRequestMessage.Headers.TryAddWithoutValidation("Accept", "application/json");

					var content = new StringContent(body);
					content.Headers.ContentType.MediaType = "application/json";
					httpRequestMessage.Content = content;

					var res = await _httpClient.SendAsync(httpRequestMessage);
					res.EnsureSuccessStatusCode();
					return DataFlowResult.Success;
				}
				catch (Exception ex)
				{
					Logger.LogError($"Save {context.Response.Request.Url} response to HBase failed [{i}]: {ex}");
				}
			}

			return DataFlowResult.Failed;
		}

		public override async Task InitAsync()
		{
			try
			{
				var body =
					"<?xml version=\"1.0\" encoding=\"UTF-8\"?><TableSchema name=\"dotnetspider:spider_response\"><ColumnSchema name=\"response\"/></TableSchema>";
				Logger.LogInformation($"Try create table: {_createTableUrl}");
				var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, _createTableUrl);
				httpRequestMessage.Headers.TryAddWithoutValidation("Accept", "text/xml");

				var content = new StringContent(body);
				content.Headers.ContentType.MediaType = "text/xml";
				httpRequestMessage.Content = content;

				var res = await _httpClient.SendAsync(httpRequestMessage);
				res.EnsureSuccessStatusCode();
			}
			catch (Exception e)
			{
				Logger.LogError($"Create table failed: {e}");
			}

			await base.InitAsync();
		}

		class Row
		{
			/// <summary>
			///
			/// </summary>
			[JsonProperty(PropertyName = "key")]
			public string Key { get; set; }

			/// <summary>
			///
			/// </summary>
			[JsonProperty(PropertyName = "Cell")]
			public List<Cell> Cell { get; set; }
		}

		class Cell
		{
			/// <summary>
			///
			/// </summary>
			[JsonProperty(PropertyName = "column")]
			public string Column { get; set; }

			/// <summary>
			///
			/// </summary>
			[JsonProperty(PropertyName = "$")]
			public string Value { get; set; }
		}
	}
}

using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Redial;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Polly.Retry;
using Polly;
using DotnetSpider.Core;
using System.Security.Cryptography;
using System.IO;
using MessagePack;

namespace DotnetSpider.Extension.Pipeline
{
	public class HttpMySqlEntityPipeline : MySqlEntityPipeline
	{
		private readonly string _api;
		private readonly RetryPolicy _retryPolicy;
		private readonly ICryptoTransform _cryptoTransform;

		public HttpMySqlEntityPipeline(string api = null)
		{
			if (string.IsNullOrEmpty(api) || string.IsNullOrWhiteSpace(api))
			{
				_api = Env.EnterpiseServicePipelineUrl;
			}
			else
			{
				_api = api;
			}

			_retryPolicy = Policy.Handle<Exception>().Retry(5, (ex, count) =>
			{
				Logger.Error($"Pipeline execute error [{count}]: {ex}");
			});

			DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
			var bytes = Encoding.ASCII.GetBytes(Env.SqlEncryptCode);
			_cryptoTransform = cryptoProvider.CreateEncryptor(bytes, bytes);
		}


		internal override void InitDatabaseAndTable()
		{
			_retryPolicy.Execute(() =>
			{
				NetworkCenter.Current.Execute("httpPipeline", () =>
				{
					foreach (var adapter in EntityAdapters.Values)
					{
						var sql = GenerateIfDatabaseExistsSql(adapter);

						if (ExecuteHttpSql(sql) == 0)
						{
							sql = GenerateCreateDatabaseSql(adapter);
							ExecuteHttpSql(sql);
						}

						sql = GenerateCreateTableSql(adapter);
						ExecuteHttpSql(sql);
					}
				});
			});
		}

		public override int Process(string entityName, List<dynamic> datas)
		{
			if (datas == null || datas.Count == 0)
			{
				return 0;
			}
			int count = 0;

			if (EntityAdapters.TryGetValue(entityName, out var metadata))
			{
				if (datas == null || datas.Count == 0)
				{
					return 0;
				}

				string sql = string.Empty;

				switch (metadata.PipelineMode)
				{
					case PipelineMode.Insert:
						{
							sql = metadata.InsertSql;
							break;
						}
					case PipelineMode.InsertAndIgnoreDuplicate:
						{
							sql = metadata.InsertAndIgnoreDuplicateSql;
							break;
						}
					case PipelineMode.InsertNewAndUpdateOld:
						{
							sql = metadata.InsertNewAndUpdateOldSql;
							break;
						}
					case PipelineMode.Update:
						{
							sql = metadata.UpdateSql;
							break;
						}
					default:
						{
							sql = metadata.InsertSql;
							break;
						}
				}

				count = ExecuteHttpSql(sql, datas);
			}
			return count;
		}

		protected int ExecuteHttpSql(string sql, dynamic data = null)
		{
			MemoryStream ms = new MemoryStream();
			CryptoStream cst = new CryptoStream(ms, _cryptoTransform, CryptoStreamMode.Write);

			StreamWriter sw = new StreamWriter(cst);
			sw.Write(sql);
			sw.Flush();
			cst.FlushFinalBlock();
			sw.Flush();

			string cryptoSql = Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length);
			var json = JsonConvert.SerializeObject(new HttpPipelinePackage
			{
				Sql = cryptoSql,
				Dt = data,
				D = Core.Infrastructure.Database.Database.MySql
			});

			var encodingBytes = Encoding.UTF8.GetBytes(json);
			var bytes = LZ4MessagePackSerializer.ToLZ4Binary(new ArraySegment<byte>(encodingBytes));
			HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, _api);
			httpRequestMessage.Headers.Add("DotnetSpiderToken", Env.EnterpiseServiceToken);
			httpRequestMessage.Content = new ByteArrayContent(bytes);
			var response = HttpSender.Client.SendAsync(httpRequestMessage).Result;
			response.EnsureSuccessStatusCode();

			return Convert.ToInt16(response.Content.ReadAsStringAsync().Result);
		}
	}
}

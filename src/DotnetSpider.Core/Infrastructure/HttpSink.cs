using Newtonsoft.Json;
using Serilog;
using Serilog.Configuration;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
#if !NET40
using System.Threading.Tasks;
#endif
using DotnetSpider.Downloader;

namespace DotnetSpider.Core.Infrastructure
{
	/// <summary>
	/// Adds the WriteTo.MSSqlServer() extension method to <see cref="LoggerConfiguration"/>.
	/// </summary>
	public static class LoggerConfigurationHttpExtensions
	{
		public static LoggerConfiguration Http(this LoggerSinkConfiguration loggerConfiguration,
			string api,
			string token,
			LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
			int batchPostingLimit = HttpSink.DefaultBatchPostingLimit,
			TimeSpan? period = null)
		{
			if (loggerConfiguration == null) throw new SpiderException($"{nameof(loggerConfiguration)} should not be null.");

			var defaultedPeriod = period ?? HttpSink.DefaultPeriod;

			return loggerConfiguration.Sink(
				new HttpSink(
					api,
					token,
					batchPostingLimit,
					defaultedPeriod),
				restrictedToMinimumLevel);
		}
	}

#if !NET40
	public class HttpSink : PeriodicBatchingSink
	{
		private readonly string _api;
		private readonly string _token;

		/// <summary>
		/// A reasonable default for the number of events posted in
		/// each batch.
		/// </summary>
		public const int DefaultBatchPostingLimit = 50;

		/// <summary>
		/// A reasonable default time to wait between checking for event batches.
		/// </summary>
		public static readonly TimeSpan DefaultPeriod = TimeSpan.FromSeconds(5);

		public HttpSink(string api, string token, int batchSizeLimit, TimeSpan period) : base(batchSizeLimit, period)
		{
			_api = api;
			_token = token;
		}

		protected override async Task EmitBatchAsync(IEnumerable<LogEvent> events)
		{
			if (events == null)
			{
				return;
			}

			int count = 0;
			try
			{
				var logs = new List<dynamic>();
				foreach (var ev in events)
				{
					count++;
					var identity = ev.Properties["Identity"].ToString();
					identity = identity.Substring(1, identity.Length - 2);
					var nodeId = ev.Properties["NodeId"].ToString();
					nodeId = nodeId.Substring(1, nodeId.Length - 2);

					var logInfo = new
					{
						Exception = ev.Exception?.ToString(),
						Identity = identity,
						Level = ev.Level.ToString(),
						Logged = ev.Timestamp,
						Message = ev.RenderMessage(),
						NodeId = nodeId,
						DateTime = DateTime.Now
					};
					logs.Add(logInfo);
				}

				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, _api);
				httpRequestMessage.Headers.Add("DotnetSpiderToken", _token);
				var json = JsonConvert.SerializeObject(logs);
				httpRequestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

				await HttpClientDownloader.Default.SendAsync(httpRequestMessage);
			}
			catch (Exception ex)
			{
				SelfLog.WriteLine("Unable to write {0} log events to the database due to following error: {1}", count,
					ex.Message);
			}
		}
	}
#else
	public class HttpSink : PeriodicBatchingSink
	{
		private readonly string _api;
		private readonly string _token;

		/// <summary>
		/// A reasonable default for the number of events posted in
		/// each batch.
		/// </summary>
		public const int DefaultBatchPostingLimit = 50;

		/// <summary>
		/// A reasonable default time to wait between checking for event batches.
		/// </summary>
		public static readonly TimeSpan DefaultPeriod = TimeSpan.FromSeconds(5);

		public HttpSink(string api, string token, int batchSizeLimit, TimeSpan period) : base(batchSizeLimit, period)
		{
			_api = api;
			_token = token;
		}

		protected override void EmitBatch(IEnumerable<LogEvent> events)
		{
			if (events == null)
			{
				return;
			}

			int count = 0;
			try
			{
				var logs = new List<dynamic>();
				foreach (var ev in events)
				{
					count++;
					var identity = ev.Properties["Identity"].ToString();
					identity = identity.Substring(1, identity.Length - 2);
					var nodeId = ev.Properties["NodeId"].ToString();
					nodeId = nodeId.Substring(1, nodeId.Length - 2);

					var logInfo = new
					{
						Exception = ev.Exception?.ToString(),
						Identity = identity,
						Level = ev.Level.ToString(),
						Logged = ev.Timestamp,
						Message = ev.RenderMessage(),
						NodeId = nodeId,
						DateTime = DateTime.Now
					};
					logs.Add(logInfo);
				}

				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, _api);
				httpRequestMessage.Headers.Add("DotnetSpiderToken", _token);
				var json = JsonConvert.SerializeObject(logs);
				httpRequestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

				DotnetSpider.Downloader.Downloader.Default.SendAsync(httpRequestMessage).Wait();
			}
			catch (Exception ex)
			{
				SelfLog.WriteLine("Unable to write {0} log events to the database due to following error: {1}", count,
					ex.Message);
			}
		}
	}
#endif
}
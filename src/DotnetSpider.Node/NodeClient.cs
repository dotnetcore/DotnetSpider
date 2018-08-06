using DotnetSpider.Common;
using DotnetSpider.Common.Dto;
using DotnetSpider.Downloader;
using LZ4;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotnetSpider.Node
{
	public class NodeClient : IDisposable
	{
		private readonly NodeOptions _options;
		private readonly HashSet<string> _runnings = new HashSet<string>();
#if !NET40
		private readonly HttpClientDownloader _downloader = new HttpClientDownloader();
#else
		private readonly HttpWebRequestDownloader _downloader = new HttpWebRequestDownloader();
#endif
		private readonly string _heartbeatUrl;
		private readonly string _pushBlockinputUrl;
		private int _retryTimes;
		private NodeStatus _status;
		private readonly Site _brokerSite;
		private readonly ThreadCommonPool _threadPool;

		/// <summary>
		/// 连接服务重试次数, 每隔一秒重试一次
		/// </summary>
		public int RetryTimes { get; set; } = 3600;

		public NodeClient()
		{
			_options = new NodeOptions();
			_options.Broker = ConfigurationManager.AppSettings["broker"].Trim();
			_options.Group = ConfigurationManager.AppSettings["group"].Trim();
			_options.Heartbeat = int.Parse(ConfigurationManager.AppSettings["heartbeat"].Trim()) * 1000;
			if (int.TryParse(ConfigurationManager.AppSettings["processCount"]?.Trim(), out int processCount))
			{
				_options.ProcessCount = processCount;
			}
			_options.RetryDownload = int.Parse(ConfigurationManager.AppSettings["retryDownload"].Trim());
			_options.Token = ConfigurationManager.AppSettings["token"]?.Trim();
			if (int.TryParse(ConfigurationManager.AppSettings["rertyPush"]?.Trim(), out int rertyPush))
			{
				_options.RertyPush = rertyPush;
			}
			_heartbeatUrl = $"{_options.Broker}api/node/heartbeat";
			_pushBlockinputUrl = $"{_options.Broker}api/node/blockinput";
			_brokerSite = new Site { Accept = "application/json, text/plain, */*", ContentType = ContentType.Json, EncodingName = "UTF-8", Headers = new Dictionary<string, string> { { "User-Agent", "DOTNETSPIDER.NODE" }, { "Token", _options.Token }, { "Accept-Encoding", "gzip" }, { "Content-Type", "application/json" } } };

			ThreadPool.SetMinThreads(256, 256);
			_threadPool = new ThreadCommonPool(_options.ProcessCount.Value);
		}

		public void Start()
		{
			_status = NodeStatus.Running;
			Polling();
		}

		private void Polling()
		{
			bool exit = false;
			while (!exit)
			{
				try
				{
					switch (_status)
					{
						case NodeStatus.Exiting:
						case NodeStatus.Exited:
							{
								exit = true;
								break;
							}
						case NodeStatus.Running:
							{
								Heartbeart();
								break;
							}
						case NodeStatus.Stop:
							{
								break;
							}
					}

					Thread.Sleep(_options.Heartbeat);
				}
				catch (Exception e)
				{
					Log.Logger.Error(e.ToString());
					Thread.Sleep(1000);
					_retryTimes++;
					if (_retryTimes <= RetryTimes)
					{
						continue;
					}
				}
			}
			_status = NodeStatus.Exited;
		}

		private void Heartbeart()
		{
			var response = SendHeartbeartRequest();
			// 请求成功则请求次数清零
			_retryTimes = 0;

			if (response == null || string.IsNullOrWhiteSpace(response.Content))
			{
				Log.Logger.Information($"Receive empty block.");
				return;
			}
			var block = JsonConvert.DeserializeObject<BlockOutput>(response.Content);
			if (string.IsNullOrWhiteSpace(block.Id) || string.IsNullOrWhiteSpace(block.Identity))
			{
				Log.Logger.Warning($"Receive unvalid block.");
				return;
			}
			switch (block.Command)
			{
				case Command.None:
					{
						Log.Logger.Warning($"Receive unvalid block {block.Id}.");
						break;
					}
				case Command.Pause:
					{
						_status = NodeStatus.Stop;
						break;
					}
				case Command.Continue:
					{
						_status = NodeStatus.Running;
						break;
					}
				case Command.Exit:
					{
						_status = NodeStatus.Exiting;
						break;
					}
				case Command.Download:
					{
						Log.Logger.Information($"Receive block {block.Id}, identity {block.Identity}.");
						_threadPool.QueueUserWork(() =>
						{
							HandleBlock(block);
						});
						break;
					}
			}
		}

		private Response SendHeartbeartRequest()
		{
			var heartbeat = new NodeHeartbeatInput
			{
				Cpu = (int)EnvironmentUtil.GetCpuLoad(),
				CpuCount = Environment.ProcessorCount,
				FreeMemory = EnvironmentUtil.GetFreeMemory(),
				Group = _options.Group,
				Ip = EnvironmentUtil.IpAddress,
				NodeId = NodeId.Id,
				Os = EnvironmentUtil.OSDescription,
				Runnings = _runnings.ToArray(),
				TotalMemory = EnvironmentUtil.TotalMemory
			};
			Log.Logger.Information(heartbeat.ToString());
			var response = _downloader.Download(new Request(_heartbeatUrl) { Method = HttpMethod.Post, Site = _brokerSite, Content = JsonConvert.SerializeObject(heartbeat) });
			return response;
		}

		private void HandleBlock(BlockOutput block)
		{
			_runnings.Add(block.Identity);
			BlockInput blockinput = new BlockInput { Id = block.Id, Identity = block.Identity, Results = new List<RequestInput>() };
			try
			{
				if (block.Requests == null || block.Requests.Count == 0)
				{
					return;
				}

				Parallel.ForEach(block.Requests, new ParallelOptions { MaxDegreeOfParallelism = block.ThreadNum }, (dto) =>
				{
					for (int i = 0; i < _options.RetryDownload; ++i)
					{
						var requestId = dto.Identity;
						try
						{
							var response = _downloader.Download(dto.ToRequest());
							lock (blockinput)
							{
								blockinput.Results.Add(new RequestInput { CycleTriedTimes = i, Content = response.Content, Identity = requestId, ResponseTime = DateTime.Now, StatusCode = (int)response.StatusCode, TargetUrl = response.TargetUrl });
							}
							Log.Logger.Information($"Request {requestId} success.");
							break;
						}
						catch (Exception e)
						{
							Log.Logger.Error($"Request {requestId} failed: {e}");
							Thread.Sleep(300);
						}
					}

					Thread.Sleep(block.Site.SleepTime);
				});
			}
			catch (Exception e)
			{
				blockinput.Exception = e.ToString();
			}
			finally
			{
				_runnings.Remove(block.Identity);

				var json = JsonConvert.SerializeObject(blockinput);
				for (int j = 0; j < _options.RertyPush; ++j)
				{
					try
					{
						var request = new Request(_pushBlockinputUrl);
						request.Method = HttpMethod.Post;
						request.Site = _brokerSite;
						request.Content = json;
						request.CompressMode = CompressMode.Lz4;
						_downloader.Download(request);
						Log.Logger.Information($"Push blockinput {block.Id}, identity {block.Identity} success.");
						break;
					}
					catch (Exception e)
					{
						Log.Logger.Error($"Push blockinput failed: {e}.");
					}
				}
			}
		}
#if !NET40
		public async Task StartAsnc()
		{
			await Task.Factory.StartNew(Start);
		}
#endif
		public void Dispose()
		{
			_status = NodeStatus.Exiting;
			Log.Logger.Information("Waiting service exit...");
			while (_status != NodeStatus.Exited)
			{
				Thread.Sleep(100);
			}
			Log.Logger.Information("Exited.");
		}
	}
}

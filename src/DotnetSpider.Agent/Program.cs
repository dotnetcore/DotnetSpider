using StackExchange.Redis;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using NLog;
using NLog.Config;

namespace DotnetSpider.Agent
{
	public class Program
	{
		public class CommandInfo
		{
			public string Id { get; set; }
			public string Target { get; set; }
			public string Command { get; set; }
			public string Data { get; set; }

			public override string ToString()
			{
				return JsonConvert.SerializeObject(this);
			}
		}

		public class NodeInfo
		{
			public string Mac { get; set; } = "MAC0001";
			public string Ip { get; set; } = "192.168.199.100";
			public int Cl { get; set; } = 50;
			public int Fm { get; set; } = 500;
			public int Tm { get; set; } = 1024;
			public string HostName { get; set; }

			public override string ToString()
			{
				return JsonConvert.SerializeObject(this);
			}

			public static NodeInfo Current()
			{
				return new NodeInfo();
			}
		}

		public class CommandResult
		{
			public string Id { get; set; }
			public bool Success { get; set; }
			public string Message { get; set; }

			public override string ToString()
			{
				return JsonConvert.SerializeObject(this);
			}
		}

		private static IConfigurationRoot _configurationRoot;
		private static ILogger _logger = LogManager.GetCurrentClassLogger();
		private static bool _exited;

		public static void Main(string[] args)
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			string nlogConfigPath = Path.Combine(AppContext.BaseDirectory, "nlog.config");
			if (!File.Exists(nlogConfigPath))
			{
				_logger.Error("Please set nlog.config.");
				Console.WriteLine("Enter any key to exit:");
				Console.Read();
				return;
			}
			LogManager.Configuration = new XmlLoggingConfiguration(nlogConfigPath);
			_logger = LogManager.GetCurrentClassLogger();

			string configPath = Path.Combine(AppContext.BaseDirectory, "config.ini");
			if (!File.Exists(configPath))
			{
				_logger.Error("Please set config.ini.");
				Console.WriteLine("Enter any key to exit:");
				Console.Read();
				return;
			}

			IConfigurationBuilder builder = new ConfigurationBuilder();
			builder.AddIniFile("config.ini");
			_configurationRoot = builder.Build();

			_logger.Info("[1] Read config success.");

			string agentIdPath = Path.Combine(AppContext.BaseDirectory, "agent.id");
			string agentId;
			if (File.Exists(agentIdPath))
			{
				agentId = File.ReadAllText(agentIdPath).Trim();
				_logger.Info("[2] Read app key success.");
			}
			else
			{
				agentId = Guid.NewGuid().ToString("N");
				File.AppendAllText(agentIdPath, agentId);
				_logger.Info("[2] Create app key success.");
			}

			var confiruation = new ConfigurationOptions
			{
				ServiceName = "DotnetSpider",
				Password = _configurationRoot.GetValue<string>("redisPassword"),
				ConnectTimeout = 5000,
				KeepAlive = 8,
				ConnectRetry = 20,
				SyncTimeout = 65530,
				ResponseTimeout = 65530
			};

			int port = _configurationRoot.GetValue<int>("redisPort");
			var host = _configurationRoot.GetValue<string>("redisHost");
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				// Lewis: This is a Workaround for .NET CORE can't use EndPoint to create Socket.

				var address = Dns.GetHostAddressesAsync(host).Result.FirstOrDefault();
				if (address == null)
				{
					_logger.Error("Can't resovle your host: " + host);
					Console.WriteLine("Enter any key to exit:");
					Console.Read();
					return;
				}
				confiruation.EndPoints.Add(new IPEndPoint(address, port));
			}
			else
			{
				confiruation.EndPoints.Add(new DnsEndPoint(host, port));
			}

			ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(confiruation);
			var db = redis.GetDatabase(2);
			var hostName = Dns.GetHostName();
			var subscriber = redis.GetSubscriber();

			_logger.Info("[3] Connect to redis server success.");

			subscriber.Subscribe($"DOTNETSPIDER_AGENT_{agentId}", (chanel, msg) =>
			{
				CommandInfo commandInfo = null;
				try
				{
					commandInfo = JsonConvert.DeserializeObject<CommandInfo>(msg);
					switch (commandInfo.Command)
					{
						case "EXECUTE":
							{
								ExecuteShellScript(commandInfo.Data);
								break;
							}
					}
					subscriber.Publish(commandInfo.Id, new CommandResult
					{
						Id = commandInfo.Id,
						Success = true
					}.ToString());
				}
				catch (Exception e)
				{
					if (commandInfo != null)
					{
						subscriber.Publish(commandInfo.Id, new CommandResult
						{
							Id = commandInfo.Id,
							Success = false,
							Message = e.Message
						}.ToString());
					}
					_logger.Error(e, e.ToString());
				}
			});

			_logger.Info("[4] Start report node info.");

			Task.Factory.StartNew(() =>
			{
				while (!_exited)
				{
					var msg = new NodeInfo
					{
						HostName = hostName
					}.ToString();
					db.HashSet("DOTNETSPIDER_NODES", agentId, msg);
					_logger.Warn(msg);
					Thread.Sleep(2000);
				}
			});

			Console.WriteLine("Enter any key to exit:");
			Console.Read();
			_exited = true;
			_logger.Info("Wait to exit...");
			Thread.Sleep(5000);
			_logger.Info("Exit success.");
		}

		public static void ExecuteShellScript(string path)
		{
			StreamReader output = null;
			try
			{
				if (!File.Exists(path))
				{
					return;
				}
				Process p = new Process
				{
					StartInfo =
					{
						FileName = path,
						UseShellExecute = false,
						RedirectStandardOutput = true,
						CreateNoWindow = true
					}
				};
				p.Start();
				output = p.StandardOutput;
				p.WaitForExit();
			}
			finally
			{
				if (output != null)
				{
					_logger.Info(output.ReadToEnd());
					output.Dispose();
				}
			}
		}
	}
}

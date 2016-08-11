using DotnetSpider.Core;
using DotnetSpider.Core.Scheduler;
using DotnetSpider.Core.Scheduler.Component;
using DotnetSpider.Core.Common;
using Newtonsoft.Json;
using System.Collections.Generic;
using StackExchange.Redis;
using System.Net;
using System.Runtime.InteropServices;
#if NET_CORE
using System.Linq;
#endif

namespace DotnetSpider.Extension.Scheduler
{
	/// <summary>
	/// Use Redis as url scheduler for distributed crawlers.
	/// </summary>
	public sealed class RedisScheduler : DuplicateRemovedScheduler, IDuplicateRemover
	{
		public string Host { get; set; }
		public int Port { get; set; } = 6379;
		public string Password { get; set; }

		public static string TaskList = "task";
		public static string TaskStatus = "task-status";
		private string _queueKey = "queue-";
		private string _setKey = "set-";
		private string _itemKey = "item-";
		private string _errorCountKey = "error-record";
		private string _successCountKey = "success-record";
		private string _identityMd5;

		private IDatabase Db;
		private ConnectionMultiplexer Redis;

		public RedisScheduler()
		{
			DuplicateRemover = this;
		}

		public RedisScheduler(string host, string password = null, int port = 6379) : this()
		{
			Host = host;
			Password = password;
			Port = port;
		}

		public override void Init(ISpider spider)
		{
			base.Init(spider);

			var confiruation = new ConfigurationOptions()
			{
				ServiceName = "DotnetSpider",
				Password = Password,
				ConnectTimeout = 5000,
				KeepAlive = 8,
				ConnectRetry = 20,
				SyncTimeout = 65530,
				ResponseTimeout = 65530
			};
#if NET_CORE
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				// Lewis: This is a Workaround for .NET CORE can't use EndPoint to create Socket.
				var address = Dns.GetHostAddressesAsync(Host).Result.FirstOrDefault();
				if (address == null)
				{
					throw new SpiderException("Can't resovle your host: " + Host);
				}
				confiruation.EndPoints.Add(new IPEndPoint(address, Port));
			}
			else
			{
				confiruation.EndPoints.Add(new DnsEndPoint(Host, Port));
			}
#else
			confiruation.EndPoints.Add(new DnsEndPoint(Host, Port));
#endif
			Redis = ConnectionMultiplexer.Connect(confiruation);
			Db = Redis.GetDatabase(0);

			var md5 = Encrypt.Md5Encrypt(spider.Identity);
			_itemKey += md5;
			_setKey += md5;
			_queueKey = md5;
			_errorCountKey += md5;
			_successCountKey += md5;
			_identityMd5 = md5;

			NetworkCenter.Current.Execute("rds-in", () =>
			{
				Db.SortedSetAdd(TaskList, spider.Identity, (long)DateTimeUtils.GetCurrentTimeStamp());
			});
		}

		public override void ResetDuplicateCheck()
		{
			NetworkCenter.Current.Execute("rds-rs", () =>
			{
				Db.KeyDelete(_setKey);
			});
		}

		public bool IsDuplicate(Request request)
		{
			return RetryExecutor.Execute(30, () =>
			{
				bool isDuplicate = Db.SetContains(_setKey, request.Identity);
				if (!isDuplicate)
				{
					Db.SetAdd(_setKey, request.Identity);
				}
				return isDuplicate;
			});
		}

		//[MethodImpl(MethodImplOptions.Synchronized)]
		protected override void PushWhenNoDuplicate(Request request)
		{
			RetryExecutor.Execute(30, () =>
			{
				Db.ListRightPush(_queueKey, request.Identity);
				string field = request.Identity;
				string value = JsonConvert.SerializeObject(request);

				Db.HashSet(_itemKey, field, value);
			});
		}

		//[MethodImpl(MethodImplOptions.Synchronized)]
		public override Request Poll()
		{
			return NetworkCenter.Current.Execute("rds-pl", () =>
			{
				return RetryExecutor.Execute(30, () =>
				{
					var value = Db.ListRightPop(_queueKey);
					if (!value.HasValue)
					{
						return null;
					}
					string field = value.ToString();

					string json = Db.HashGet(_itemKey, field);

					if (!string.IsNullOrEmpty(json))
					{
						var result = JsonConvert.DeserializeObject<Request>(json);
						Db.HashDelete(_itemKey, field);
						return result;
					}
					return null;
				});
			});
		}

		public override long GetLeftRequestsCount()
		{
			return NetworkCenter.Current.Execute("rds-lc", () => Db.ListLength(_queueKey));
		}

		public override long GetTotalRequestsCount()
		{
			return NetworkCenter.Current.Execute("rds-tc", () => Db.SetLength(_setKey));
		}

		public override long GetSuccessRequestsCount()
		{
			return NetworkCenter.Current.Execute("rds-src", () =>
			{
				var result = Db.HashGet(_successCountKey, _identityMd5);
				return result.HasValue ? (long)result : 0;
			});
		}

		public override long GetErrorRequestsCount()
		{
			return NetworkCenter.Current.Execute("rds-erc", () =>
			{
				var result = Db.HashGet(_errorCountKey, _identityMd5);
				return result.HasValue ? (long)result : 0;
			});
		}

		public override void IncreaseSuccessCounter()
		{
			NetworkCenter.Current.Execute("rds-isc", () =>
			{
				Db.HashIncrement(_successCountKey, _identityMd5);
			});
		}

		public override void IncreaseErrorCounter()
		{
			NetworkCenter.Current.Execute("rds-iec", () =>
			{
				Db.HashIncrement(_errorCountKey, _identityMd5);
			});
		}

		public override void Dispose()
		{
			Db.KeyDelete(_successCountKey);
			Db.KeyDelete(_errorCountKey);
		}

		public override void Load(HashSet<Request> requests)
		{
			lock (this)
			{
				int cacheSize = requests.Count > 10000 ? 10000 : requests.Count;
				RedisValue[] identities = new RedisValue[cacheSize];
				HashEntry[] items = new HashEntry[cacheSize];
				int i = 0;
				int j = requests.Count % 10000;
				int n = requests.Count / 10000;

				foreach (var request in requests)
				{
					identities[i] = request.Identity;
					items[i] = new HashEntry(request.Identity, JsonConvert.SerializeObject(request));
					++i;
					if (i == 10000)
					{
						--n;

						Db.SetAdd(_setKey, identities);
						Db.ListRightPush(_queueKey, identities);
						Db.HashSet(_itemKey, items);

						i = 0;
						if (n != 0)
						{
							identities = new RedisValue[10000];
							items = new HashEntry[10000];
						}
						else
						{
							identities = new RedisValue[j];
							items = new HashEntry[j];
						}
					}
				}

				if (i > 0)
				{
					Db.SetAdd(_setKey, identities);
					Db.ListRightPush(_queueKey, identities);
					Db.HashSet(_itemKey, items);
				}
			}
		}

		public override HashSet<Request> ToList()
		{
			HashSet<Request> requests = new HashSet<Request>();
			Request request;
			while ((request = Poll()) != null)
			{
				requests.Add(request);
			}
			return requests;
		}

		public override void Clear()
		{
			base.Clear();

			Db.KeyDelete(_queueKey);
			Db.KeyDelete(_setKey);
			Db.KeyDelete(_itemKey);
			Db.KeyDelete(_successCountKey);
			Db.KeyDelete(_errorCountKey);
		}
	}
}

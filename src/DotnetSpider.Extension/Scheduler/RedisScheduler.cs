using DotnetSpider.Core;
using DotnetSpider.Core.Scheduler;
using DotnetSpider.Core.Scheduler.Component;
using DotnetSpider.Core.Common;
using Newtonsoft.Json;
using System.Collections.Generic;
using StackExchange.Redis;
using System.Net;
#if NET_CORE
using System.Runtime.InteropServices;
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

		public const string TaskList = "task";
		public const string TaskStatus = "task-status";
		private string _queueKey;
		private string _setKey;
		private string _itemKey;
		private string _errorCountKey;
		private string _successCountKey;
		private string _identityMd5;

		private IDatabase _db;
		private ConnectionMultiplexer _redis;

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
				ConnectTimeout = 65530,
				KeepAlive = 20,
				ConnectRetry = 3,
				ResponseTimeout = 5000
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
			_redis = ConnectionMultiplexer.Connect(confiruation);
			_db = _redis.GetDatabase(0);

			var md5 = Encrypt.Md5Encrypt(spider.Identity);
			_itemKey = "item-" + md5;
			_setKey = "set-" + md5;
			_queueKey = "queue-" + md5;
			_errorCountKey = "error-record" + md5;
			_successCountKey = "success-record" + md5;
			_identityMd5 = md5;

			NetworkCenter.Current.Execute("rds-in", () =>
			{
				_db.SortedSetAdd(TaskList, spider.Identity, (long)DateTimeUtils.GetCurrentTimeStamp());
			});
		}

		public override void ResetDuplicateCheck()
		{
			NetworkCenter.Current.Execute("rds-rs", () =>
			{
				_db.KeyDelete(_setKey);
			});
		}

		public bool IsDuplicate(Request request)
		{
			return RetryExecutor.Execute(30, () =>
			{
				bool isDuplicate = _db.SetContains(_setKey, request.Identity);
				if (!isDuplicate)
				{
					_db.SetAdd(_setKey, request.Identity);
				}
				return isDuplicate;
			});
		}

		//[MethodImpl(MethodImplOptions.Synchronized)]
		protected override void PushWhenNoDuplicate(Request request)
		{
			RetryExecutor.Execute(30, () =>
			{
				_db.ListRightPush(_queueKey, request.Identity);
				string field = request.Identity;
				string value = JsonConvert.SerializeObject(request);

				_db.HashSet(_itemKey, field, value);
			});
		}

		//[MethodImpl(MethodImplOptions.Synchronized)]
		public override Request Poll()
		{
			return NetworkCenter.Current.Execute("rds-pl", () =>
			{
				return RetryExecutor.Execute(30, () =>
				{
					RedisValue value;
					if (DepthFirst)
					{
						value = _db.ListRightPop(_queueKey);
					}
					else
					{
						value = _db.ListLeftPop(_queueKey);
					}
					if (!value.HasValue)
					{
						return null;
					}
					string field = value.ToString();

					string json = _db.HashGet(_itemKey, field);

					if (!string.IsNullOrEmpty(json))
					{
						var result = JsonConvert.DeserializeObject<Request>(json);
						_db.HashDelete(_itemKey, field);
						return result;
					}
					return null;
				});
			});
		}

		public override long GetLeftRequestsCount()
		{
			return NetworkCenter.Current.Execute("rds-lc", () => _db.ListLength(_queueKey));
		}

		public override long GetTotalRequestsCount()
		{
			return NetworkCenter.Current.Execute("rds-tc", () => _db.SetLength(_setKey));
		}

		public override long GetSuccessRequestsCount()
		{
			return NetworkCenter.Current.Execute("rds-src", () =>
			{
				var result = _db.HashGet(_successCountKey, _identityMd5);
				return result.HasValue ? (long)result : 0;
			});
		}

		public override long GetErrorRequestsCount()
		{
			return NetworkCenter.Current.Execute("rds-erc", () =>
			{
				var result = _db.HashGet(_errorCountKey, _identityMd5);
				return result.HasValue ? (long)result : 0;
			});
		}

		public override void IncreaseSuccessCounter()
		{
			NetworkCenter.Current.Execute("rds-isc", () =>
			{
				_db.HashIncrement(_successCountKey, _identityMd5);
			});
		}

		public override void IncreaseErrorCounter()
		{
			NetworkCenter.Current.Execute("rds-iec", () =>
			{
				_db.HashIncrement(_errorCountKey, _identityMd5);
			});
		}

		public override void Dispose()
		{
			_db.KeyDelete(_queueKey);
			_db.KeyDelete(_setKey);
			_db.KeyDelete(_itemKey);
			_db.KeyDelete(_successCountKey);
			_db.KeyDelete(_errorCountKey);
		}

		public override void Import(HashSet<Request> requests)
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

						_db.SetAdd(_setKey, identities);
						_db.ListRightPush(_queueKey, identities);
						_db.HashSet(_itemKey, items);

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
					_db.SetAdd(_setKey, identities);
					_db.ListRightPush(_queueKey, identities);
					_db.HashSet(_itemKey, items);
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

		public override bool IsExited
		{
			get
			{
				var result = _db.HashGet(TaskStatus, _identityMd5);
				if (result.HasValue)
				{
					return result == 1;
				}
				else
				{
					return false;
				}
			}
			set
			{
				_db.HashSet(TaskStatus, _identityMd5, value ? 1 : 0);
			}
		}

		#region For Test

		public string GetQueueKey()
		{
			return _queueKey;
		}

		public string GetSetKey()
		{
			return _setKey;
		}

		public string GetItemKey()
		{
			return _itemKey;
		}

		public string GetErrorCountKey()
		{
			return _errorCountKey;
		}

		public string GetSuccessCountKey()
		{
			return _successCountKey;
		}

		#endregion
	}
}

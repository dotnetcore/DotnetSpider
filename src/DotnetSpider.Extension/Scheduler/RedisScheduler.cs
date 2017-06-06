using DotnetSpider.Core;
using DotnetSpider.Core.Scheduler;
using DotnetSpider.Core.Scheduler.Component;
using DotnetSpider.Core.Infrastructure;
using Newtonsoft.Json;
using System.Collections.Generic;
using StackExchange.Redis;
using DotnetSpider.Extension.Infrastructure;
#if NET_CORE
#endif

namespace DotnetSpider.Extension.Scheduler
{
	/// <summary>
	/// Use Redis as url scheduler for distributed crawlers.
	/// </summary>
	public sealed class RedisScheduler : DuplicateRemovedScheduler, IDuplicateRemover
	{
		public string ConnectString { get; }

		public RedisConnection RedisConnection { get; private set; }

		public const string TasksKey = "dotnetspider:tasks";
		public const string TaskStatsKey = "dotnetspider:task-stats";
		private string _queueKey;
		private string _setKey;
		private string _itemKey;
		private string _errorCountKey;
		private string _successCountKey;
		private string _identityMd5;

		private RedisScheduler()
		{
			DuplicateRemover = this;
		}

		public RedisScheduler(string connectString) : this()
		{
			ConnectString = connectString;
		}

		public override void Init(ISpider spider)
		{
			base.Init(spider);

			RedisConnection = Cache.Instance.Get(ConnectString);
			if (RedisConnection == null)
			{
				RedisConnection = new RedisConnection(ConnectString);
				Cache.Instance.Set(ConnectString, RedisConnection);
			}

			var md5 = Encrypt.Md5Encrypt(spider.Identity);
			_itemKey = $"dotnetspider:scheduler:{md5}:items";
			_setKey = $"dotnetspider:scheduler:{md5}:set";
			_queueKey = $"dotnetspider:scheduler:{md5}:queue";
			_errorCountKey = $"dotnetspider:scheduler:{md5}:numberOfFailures";
			_successCountKey = $"dotnetspider:scheduler:{md5}:numberOfSuccessful";

			_identityMd5 = md5;

			NetworkCenter.Current.Execute("rds-in", () =>
			{
				RedisConnection.Database.SortedSetAdd(TasksKey, spider.Identity, (long)DateTimeUtils.GetCurrentTimeStamp());
			});
		}

		public override void ResetDuplicateCheck()
		{
			NetworkCenter.Current.Execute("rds-rs", () =>
			{
				RedisConnection.Database.KeyDelete(_setKey);
			});
		}

		public bool IsDuplicate(Request request)
		{
			return RetryExecutor.Execute(30, () =>
			{
				bool isDuplicate = RedisConnection.Database.SetContains(_setKey, request.Identity);
				if (!isDuplicate)
				{
					RedisConnection.Database.SetAdd(_setKey, request.Identity);
				}
				return isDuplicate;
			});
		}

		protected override void PushWhenNoDuplicate(Request request)
		{
			RetryExecutor.Execute(30, () =>
			{
				RedisConnection.Database.ListRightPush(_queueKey, request.Identity);
				string field = request.Identity;
				string value = JsonConvert.SerializeObject(request);

				RedisConnection.Database.HashSet(_itemKey, field, value);
			});
		}

		public override Request Poll()
		{
			return NetworkCenter.Current.Execute("rds-pl", () =>
			{
				return RetryExecutor.Execute(30, () =>
				{
					RedisValue value;
					if (DepthFirst)
					{
						value = RedisConnection.Database.ListRightPop(_queueKey);
					}
					else
					{
						value = RedisConnection.Database.ListLeftPop(_queueKey);
					}
					if (!value.HasValue)
					{
						return null;
					}
					string field = value.ToString();

					string json = RedisConnection.Database.HashGet(_itemKey, field);

					if (!string.IsNullOrEmpty(json))
					{
						var result = JsonConvert.DeserializeObject<Request>(json);
						RedisConnection.Database.HashDelete(_itemKey, field);
						return result;
					}
					return null;
				});
			});
		}

		public override long GetLeftRequestsCount()
		{
			return NetworkCenter.Current.Execute("rds-lc", () => RedisConnection.Database.ListLength(_queueKey));
		}

		public override long GetTotalRequestsCount()
		{
			return NetworkCenter.Current.Execute("rds-tc", () => RedisConnection.Database.SetLength(_setKey));
		}

		public override long GetSuccessRequestsCount()
		{
			return NetworkCenter.Current.Execute("rds-src", () =>
			{
				var result = RedisConnection.Database.HashGet(_successCountKey, _identityMd5);
				return result.HasValue ? (long)result : 0;
			});
		}

		public override long GetErrorRequestsCount()
		{
			return NetworkCenter.Current.Execute("rds-erc", () =>
			{
				var result = RedisConnection.Database.HashGet(_errorCountKey, _identityMd5);
				return result.HasValue ? (long)result : 0;
			});
		}

		public override void IncreaseSuccessCounter()
		{
			NetworkCenter.Current.Execute("rds-isc", () =>
			{
				RedisConnection.Database.HashIncrement(_successCountKey, _identityMd5);
			});
		}

		public override void IncreaseErrorCounter()
		{
			NetworkCenter.Current.Execute("rds-iec", () =>
			{
				RedisConnection.Database.HashIncrement(_errorCountKey, _identityMd5);
			});
		}

		public override void Dispose()
		{
			IsExited = true;
		}

		public override void Clean()
		{
			base.Clean();

			RedisConnection.Database.KeyDelete(_queueKey);
			RedisConnection.Database.KeyDelete(_setKey);
			RedisConnection.Database.KeyDelete(_itemKey);
			RedisConnection.Database.KeyDelete(_successCountKey);
			RedisConnection.Database.KeyDelete(_errorCountKey);
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

						RedisConnection.Database.SetAdd(_setKey, identities);
						RedisConnection.Database.ListRightPush(_queueKey, identities);
						RedisConnection.Database.HashSet(_itemKey, items);

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
					RedisConnection.Database.SetAdd(_setKey, identities);
					RedisConnection.Database.ListRightPush(_queueKey, identities);
					RedisConnection.Database.HashSet(_itemKey, items);
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
				var result = RedisConnection.Database.HashGet(TaskStatsKey, _identityMd5);
				if (result.HasValue)
				{
					return result == 1;
				}
				else
				{
					return false;
				}
			}
			set => RedisConnection.Database.HashSet(TaskStatsKey, _identityMd5, value ? 1 : 0);
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

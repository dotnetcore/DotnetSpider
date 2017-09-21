using DotnetSpider.Core;
using DotnetSpider.Core.Scheduler;
using DotnetSpider.Core.Scheduler.Component;
using DotnetSpider.Core.Infrastructure;
using Newtonsoft.Json;
using System.Collections.Generic;
using StackExchange.Redis;
using DotnetSpider.Extension.Infrastructure;
using DotnetSpider.Core.Redial;
using System;
#if NET_CORE
#endif

namespace DotnetSpider.Extension.Scheduler
{
	/// <summary>
	/// Use Redis as url scheduler for distributed crawlers.
	/// </summary>
	public sealed class RedisScheduler : DuplicateRemovedScheduler, IDuplicateRemover
	{
		private readonly object _locker = new object();
		public const string TasksKey = "dotnetspider:tasks";
		public const string TaskStatsKey = "dotnetspider:task-stats";

		private string _queueKey;
		private string _setKey;
		private string _itemKey;
		private string _errorCountKey;
		private string _successCountKey;
		private string _identityMd5;

		public string ConnectString { get; }

		public int BatchCount { get; set; } = 1000;

		public override bool UseInternet { get; set; } = true;

		public RedisConnection RedisConnection { get; private set; }

		public RedisScheduler(string connectString)
		{
			ConnectString = connectString;
			DuplicateRemover = this;
		}

		public RedisScheduler()
		{
			ConnectString = Env.RedisConnectString;
			DuplicateRemover = this;
		}

		public override void Init(ISpider spider)
		{
			base.Init(spider);

			if (string.IsNullOrEmpty(_identityMd5))
			{
				var md5 = Encrypt.Md5Encrypt(spider.Identity);
				_itemKey = $"dotnetspider:scheduler:{md5}:items";
				_setKey = $"dotnetspider:scheduler:{md5}:set";
				_queueKey = $"dotnetspider:scheduler:{md5}:queue";
				_errorCountKey = $"dotnetspider:scheduler:{md5}:numberOfFailures";
				_successCountKey = $"dotnetspider:scheduler:{md5}:numberOfSuccessful";

				_identityMd5 = md5;

				var action = new Action(() =>
				{
					RedisConnection = Cache.Instance.Get(ConnectString);
					if (RedisConnection == null)
					{
						RedisConnection = new RedisConnection(ConnectString);
						Cache.Instance.Set(ConnectString, RedisConnection);
					}
					RedisConnection.Database.SortedSetAdd(TasksKey, spider.Identity, (long)DateTimeUtils.GetCurrentTimeStamp());
				});

				if (UseInternet)
				{
					NetworkCenter.Current.Execute("rds-init", action);
				}
				else
				{
					action();
				}
			}
		}

		public override void ResetDuplicateCheck()
		{
			var action = new Action(() =>
			{
				RedisConnection.Database.KeyDelete(_setKey);
			});
			if (UseInternet)
			{
				NetworkCenter.Current.Execute("rds-reset", action);
			}
			else
			{
				action();
			}
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

		public override Request Poll()
		{
			if (UseInternet)
			{
				return NetworkCenter.Current.Execute("rds-poll", PollRequest);
			}
			else
			{
				return PollRequest();
			}
		}

		public override long LeftRequestsCount
		{
			get
			{
				if (UseInternet)
				{
					return NetworkCenter.Current.Execute("rds-left", () => RedisConnection.Database.ListLength(_queueKey));
				}
				else
				{
					return RedisConnection.Database.ListLength(_queueKey);
				}
			}
		}

		public override long TotalRequestsCount
		{
			get
			{
				if (UseInternet)
				{
					return NetworkCenter.Current.Execute("rds-total", () => RedisConnection.Database.SetLength(_setKey));
				}
				else
				{
					return RedisConnection.Database.SetLength(_setKey);
				}
			}
		}

		public override long SuccessRequestsCount
		{
			get
			{
				if (UseInternet)
				{
					return NetworkCenter.Current.Execute("rds-success", () =>
					{
						var result = RedisConnection.Database.HashGet(_successCountKey, _identityMd5);
						return result.HasValue ? (long)result : 0;
					});
				}
				else
				{
					var result = RedisConnection.Database.HashGet(_successCountKey, _identityMd5);
					return result.HasValue ? (long)result : 0;
				}
			}
		}

		public override long ErrorRequestsCount
		{
			get
			{
				if (UseInternet)
				{
					return NetworkCenter.Current.Execute("rds-error", () =>
					{
						var result = RedisConnection.Database.HashGet(_errorCountKey, _identityMd5);
						return result.HasValue ? (long)result : 0;
					});
				}
				else
				{
					var result = RedisConnection.Database.HashGet(_errorCountKey, _identityMd5);
					return result.HasValue ? (long)result : 0;
				}
			}
		}

		public override void IncreaseSuccessCount()
		{
			if (UseInternet)
			{
				NetworkCenter.Current.Execute("rds-inc-success", () =>
				{
					RedisConnection.Database.HashIncrement(_successCountKey, _identityMd5);
				});
			}
			else
			{
				RedisConnection.Database.HashIncrement(_successCountKey, _identityMd5);
			}
		}

		public override void IncreaseErrorCount()
		{
			if (UseInternet)
			{
				NetworkCenter.Current.Execute("rds-inc-error", () =>
				{
					RedisConnection.Database.HashIncrement(_errorCountKey, _identityMd5);
				});
			}
			else
			{
				RedisConnection.Database.HashIncrement(_errorCountKey, _identityMd5);
			}
		}

		public override void Dispose()
		{
			IsExited = true;
		}

		public override void Clear()
		{
			base.Clear();

			if (UseInternet)
			{
				NetworkCenter.Current.Execute("rds-inc-clear", () =>
				{
					RedisConnection.Database.KeyDelete(_queueKey);
					RedisConnection.Database.KeyDelete(_setKey);
					RedisConnection.Database.KeyDelete(_itemKey);
					RedisConnection.Database.KeyDelete(_successCountKey);
					RedisConnection.Database.KeyDelete(_errorCountKey);
				});
			}
			else
			{
				RedisConnection.Database.KeyDelete(_queueKey);
				RedisConnection.Database.KeyDelete(_setKey);
				RedisConnection.Database.KeyDelete(_itemKey);
				RedisConnection.Database.KeyDelete(_successCountKey);
				RedisConnection.Database.KeyDelete(_errorCountKey);
			}
		}

		public override void Import(HashSet<Request> requests)
		{
			var action = new Action(() =>
			{
				lock (_locker)
				{
					int batchCount = BatchCount;
					int cacheSize = requests.Count > batchCount ? batchCount : requests.Count;
					RedisValue[] identities = new RedisValue[cacheSize];
					HashEntry[] items = new HashEntry[cacheSize];
					int i = 0;
					int j = requests.Count % batchCount;
					int n = requests.Count / batchCount;

					foreach (var request in requests)
					{
						identities[i] = request.Identity;
						items[i] = new HashEntry(request.Identity, JsonConvert.SerializeObject(request));
						++i;
						if (i == batchCount)
						{
							--n;

							RedisConnection.Database.SetAdd(_setKey, identities);
							RedisConnection.Database.ListRightPush(_queueKey, identities);
							RedisConnection.Database.HashSet(_itemKey, items, CommandFlags.HighPriority);

							i = 0;
							if (n != 0)
							{
								identities = new RedisValue[batchCount];
								items = new HashEntry[batchCount];
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
			});
			if (UseInternet)
			{
				NetworkCenter.Current.Execute("rds-import", action);
			}
			else
			{
				action();
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
				try
				{
					if (UseInternet)
					{
						return NetworkCenter.Current.Execute("rds-isexited", () =>
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
						});
					}
					else
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

				}
				catch
				{
					return false;
				}
			}
			set
			{
				var action = new Action(() =>
				{
					RedisConnection.Database.HashSet(TaskStatsKey, _identityMd5, value ? 1 : 0);
				});
				if (UseInternet)
				{
					NetworkCenter.Current.Execute("rds-isexited", action);
				}
				else
				{
					action();
				}
			}
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

		private Request PollRequest()
		{
			return RetryExecutor.Execute(30, () =>
			{
				var value = DepthFirst ? RedisConnection.Database.ListRightPop(_queueKey) : RedisConnection.Database.ListLeftPop(_queueKey);

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
		}

		#region For Test

		internal string GetQueueKey()
		{
			return _queueKey;
		}

		internal string GetSetKey()
		{
			return _setKey;
		}

		internal string GetItemKey()
		{
			return _itemKey;
		}

		internal string GetErrorCountKey()
		{
			return _errorCountKey;
		}

		internal string GetSuccessCountKey()
		{
			return _successCountKey;
		}

		#endregion
	}
}

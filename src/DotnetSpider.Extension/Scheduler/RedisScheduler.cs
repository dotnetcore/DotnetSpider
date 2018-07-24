using DotnetSpider.Core;
using DotnetSpider.Core.Scheduler;
using DotnetSpider.Core.Scheduler.Component;
using DotnetSpider.Core.Infrastructure;
using Newtonsoft.Json;
using System.Collections.Generic;
using StackExchange.Redis;
using DotnetSpider.Extension.Infrastructure;
using System;
using Polly;
using Polly.Retry;
using System.Linq;
using DotnetSpider.Common;
using DotnetSpider.Downloader;

namespace DotnetSpider.Extension.Scheduler
{
	/// <summary>
	/// Use Redis as url scheduler for distributed crawlers.
	/// </summary>
	public class RedisScheduler : DuplicateRemovedScheduler, IDuplicateRemover
	{
		private readonly object _locker = new object();
		private const string TasksKey = "dotnetspider:tasks";
		private readonly RetryPolicy _retryPolicy = Policy.Handle<Exception>().Retry(30);
		private readonly string _queueKey;
		private readonly string _setKey;
		private readonly string _itemKey;
		private readonly string _errorCountKey;
		private readonly string _successCountKey;
		private readonly AutomicLong _successCounter = new AutomicLong(0);
		private readonly AutomicLong _errorCounter = new AutomicLong(0);
		private RedisConnection _redisConnection;
		private readonly Dictionary<string, RedisConnection> _cache = new Dictionary<string, RedisConnection>();

		/// <summary>
		/// 批量加载时的每批次加载数
		/// </summary>
		public int BatchCount { get; set; } = 1000;

		public override bool IsDistributed => true;

		/// <summary>
		/// RedisScheduler是否会使用互联网
		/// </summary>
		protected sealed override bool UseInternet { get; set; } = true;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="identity">对列标识</param>
		public RedisScheduler(string identity) : this(identity, Env.RedisConnectString)
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="identity">对列标识</param>
		/// <param name="connectString">Redis连接字符串</param>
		public RedisScheduler(string identity, string connectString)
		{
			if (string.IsNullOrWhiteSpace(identity))
			{
				throw new ArgumentNullException(nameof(identity));
			}
			if (string.IsNullOrWhiteSpace(connectString))
			{
				throw new ArgumentNullException(nameof(connectString));
			}

			var s = connectString;
			DuplicateRemover = this;

			var md5 = identity.ToShortMd5();
			_itemKey = $"dotnetspider:scheduler:{md5}:items";
			_setKey = $"dotnetspider:scheduler:{md5}:set";
			_queueKey = $"dotnetspider:scheduler:{md5}:queue";
			_errorCountKey = $"dotnetspider:scheduler:{md5}:countOfFailures";
			_successCountKey = $"dotnetspider:scheduler:{md5}:countOfSuccess";

			var action = new Action(() =>
			{
				if (!_cache.ContainsKey(s))
				{
					_redisConnection = new RedisConnection(s);
					_cache.Add(s, _redisConnection);
				}
				_redisConnection.Database.SortedSetAdd(TasksKey, identity, (long)DateTimeUtil.GetCurrentUnixTimeNumber());
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

		/// <summary>
		/// Reset duplicate check.
		/// </summary>
		public override void ResetDuplicateCheck()
		{
			var action = new Action(() =>
			{
				_redisConnection.Database.KeyDelete(_setKey);
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

		/// <summary>
		/// Check whether the request is duplicate.
		/// </summary>
		/// <param name="request">Request</param>
		/// <returns>Whether the request is duplicate.</returns>
		public virtual bool IsDuplicate(Request request)
		{
			return _retryPolicy.Execute(() =>
			{
				bool isDuplicate = _redisConnection.Database.SetContains(_setKey, request.Identity);
				if (!isDuplicate)
				{
					_redisConnection.Database.SetAdd(_setKey, request.Identity);
				}
				return isDuplicate;
			});
		}

		/// <summary>
		/// 取得一个需要处理的请求对象
		/// </summary>
		/// <returns>请求对象</returns>
		public override Request Poll()
		{
			if (UseInternet)
			{
				return NetworkCenter.Current.Execute("rds-poll", ImplPollRequest);
			}
			else
			{
				return ImplPollRequest();
			}
		}

		/// <summary>
		/// 剩余链接数
		/// </summary>
		public override long LeftRequestsCount
		{
			get
			{
				if (UseInternet)
				{
					return NetworkCenter.Current.Execute("rds-left", () => _redisConnection.Database.ListLength(_queueKey));
				}
				else
				{
					return _redisConnection.Database.ListLength(_queueKey);
				}
			}
		}

		/// <summary>
		/// 总的链接数
		/// </summary>
		public override long TotalRequestsCount
		{
			get
			{
				if (UseInternet)
				{
					return NetworkCenter.Current.Execute("rds-total", () => _redisConnection.Database.SetLength(_setKey));
				}
				else
				{
					return _redisConnection.Database.SetLength(_setKey);
				}
			}
		}

		/// <summary>
		/// 采集成功的链接数
		/// </summary>
		public override long SuccessRequestsCount => _successCounter.Value;

		/// <summary>
		/// 采集失败的次数, 不是链接数, 如果一个链接采集多次都失败会记录多次
		/// </summary>
		public override long ErrorRequestsCount => _errorCounter.Value;

		/// <summary>
		/// 采集成功的链接数加 1
		/// </summary>
		public override void IncreaseSuccessCount()
		{
			_successCounter.Inc();
		}

		/// <summary>
		/// 采集失败的次数加 1
		/// </summary>
		public override void IncreaseErrorCount()
		{
			_errorCounter.Inc();
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public override void Dispose()
		{
			if (UseInternet)
			{
				NetworkCenter.Current.Execute("rds-inc-clear", () =>
				{
					_redisConnection.Database.KeyDelete(_queueKey);
					_redisConnection.Database.KeyDelete(_setKey);
					_redisConnection.Database.KeyDelete(_itemKey);
					_redisConnection.Database.KeyDelete(_successCountKey);
					_redisConnection.Database.KeyDelete(_errorCountKey);
				});
			}
			else
			{
				_redisConnection.Database.KeyDelete(_queueKey);
				_redisConnection.Database.KeyDelete(_setKey);
				_redisConnection.Database.KeyDelete(_itemKey);
				_redisConnection.Database.KeyDelete(_successCountKey);
				_redisConnection.Database.KeyDelete(_errorCountKey);
			}
		}

		/// <summary>
		/// 批量导入
		/// </summary>
		/// <param name="requests">请求对象</param>
		public override void Reload(ICollection<Request> requests)
		{
			var action = new Action(() =>
			{
				lock (_locker)
				{
					int batchCount = BatchCount;

					var count = requests.Count();
					int cacheSize = count > batchCount ? batchCount : count;
					RedisValue[] identities = new RedisValue[cacheSize];
					HashEntry[] items = new HashEntry[cacheSize];
					int i = 0;
					int j = count % batchCount;
					int n = count / batchCount;

					foreach (var request in requests)
					{
						identities[i] = request.Identity;
						items[i] = new HashEntry(request.Identity, JsonConvert.SerializeObject(request));
						++i;
						if (i == batchCount)
						{
							--n;

							_redisConnection.Database.SetAdd(_setKey, identities);
							_redisConnection.Database.ListRightPush(_queueKey, identities);
							_redisConnection.Database.HashSet(_itemKey, items, CommandFlags.HighPriority);

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
						_redisConnection.Database.SetAdd(_setKey, identities);
						_redisConnection.Database.ListRightPush(_queueKey, identities);
						_redisConnection.Database.HashSet(_itemKey, items);
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

		/// <summary>
		/// 把队列中的请求对象转换成List
		/// </summary>
		/// <returns>请求对象的List</returns>
		public HashSet<Request> ToList()
		{
			HashSet<Request> requests = new HashSet<Request>();
			Request request;
			while ((request = Poll()) != null)
			{
				requests.Add(request);
			}
			return requests;
		}

		/// <summary>
		/// 如果链接不是重复的就添加到队列中
		/// </summary>
		/// <param name="request">请求对象</param>
		protected override void PushWhenNoDuplicate(Request request)
		{
			_retryPolicy.Execute(() =>
			{
				_redisConnection.Database.ListRightPush(_queueKey, request.Identity);
				string field = request.Identity;
				string value = JsonConvert.SerializeObject(request);

				_redisConnection.Database.HashSet(_itemKey, field, value);
			});
		}

		private Request ImplPollRequest()
		{
			return _retryPolicy.Execute(() =>
			{
				RedisValue value;
				switch (TraverseStrategy)
				{
					case TraverseStrategy.Dfs:
						{
							value = _redisConnection.Database.ListRightPop(_queueKey);
							break;
						}
					case TraverseStrategy.Bfs:
						{
							value = _redisConnection.Database.ListLeftPop(_queueKey);
							break;
						}
					default:
						{
							throw new NotImplementedException();
						}
				}
				if (!value.HasValue)
				{
					return null;
				}
				string field = value.ToString();

				string json = _redisConnection.Database.HashGet(_itemKey, field);

				if (!string.IsNullOrEmpty(json))
				{
					var result = JsonConvert.DeserializeObject<Request>(json);
					_redisConnection.Database.HashDelete(_itemKey, field);
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

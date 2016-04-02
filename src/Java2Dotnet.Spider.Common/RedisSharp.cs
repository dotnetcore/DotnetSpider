using System;
using System.Collections.Concurrent;
using System.IO;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Java2Dotnet.Spider.Common;

namespace RedisSharp
{
	public class RedisServer : IDisposable
	{
		private readonly BlockingQueue<RedisClient> _redisClientQueue;
		private readonly SynchronizedList<RedisClient> _allClients = new SynchronizedList<RedisClient>();
		private readonly AtomicInteger _currentQueryCount = new AtomicInteger(0);
		public string Host { get; }
		public int Port { get; }
		public int RetryTimeout { get; set; }
		public int RetryCount { get; set; }
		public int SendTimeout { get; set; }
		public string Password { get; }
		public int MaxThreadNum { get; }
		public float RateOfQueryAndClient { get; }
		public int Db { get; set; }
		private bool _isDisposing;

		public RedisServer(string host, int port, string pass, int maxThreadNum = 10, float rateOfQueryAndClient = 1F)
		{
			Host = host;
			Port = port;
			Password = pass;
			RateOfQueryAndClient = rateOfQueryAndClient;
			if (maxThreadNum > 150)
			{
				throw new Exception("We strongly suggest you don't open too much thread.");
			}
			MaxThreadNum = maxThreadNum;

			_redisClientQueue = new BlockingQueue<RedisClient>(maxThreadNum);

			var client = CreateRedisClinet();
			_allClients.Add(client);
			_redisClientQueue.Enqueue(client);

			Task.Factory.StartNew(() =>
			{
				while (true)
				{
					if (_isDisposing)
					{
						break;
					}
					CaculateRedisClientNum();
					Thread.Sleep(10);
				}
			});
		}

		public RedisServer(string host, int port) : this(host, port, null)
		{
		}

		public RedisServer(string host) : this(host, 6379)
		{
		}

		public RedisServer() : this("localhost", 6379)
		{
		}

		private void CaculateRedisClientNum()
		{
			int currentQueryCount = _currentQueryCount.Value;
			int threadCount = _allClients.Count();
			float rate = (currentQueryCount / (float)threadCount);
			if (rate > RateOfQueryAndClient)
			{
				var client = CreateRedisClinet();
				_allClients.Add(client);
				_redisClientQueue.Enqueue(client);

				Console.WriteLine($"Query: {currentQueryCount} RedisClient: {threadCount}");
			}
			else if (rate < 1 && currentQueryCount > 0)
			{
				var client = _redisClientQueue.Dequeue();
				_allClients.Remove(client);
				client.Dispose();
			}
		}

		public bool KeyDelete(string key)
		{
			_currentQueryCount.Inc();
			RedisClient client = _redisClientQueue.Dequeue();
			try
			{
				client.Db = Db;
				return client.KeyDelete(key);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				_currentQueryCount.Dec();
			}
		}

		private RedisClient CreateRedisClinet()
		{
			return new RedisClient(Host, Port) { Password = Password, RetryCount = RetryCount, RetryTimeout = RetryTimeout };
		}

		public bool ContainsKey(string key)
		{
			_currentQueryCount.Inc();
			RedisClient client = _redisClientQueue.Dequeue();
			try
			{
				client.Db = Db;
				return client.ContainsKey(key);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				_currentQueryCount.Dec();
			}
		}

		public bool LockTake(string key, string value, TimeSpan expiry)
		{
			_currentQueryCount.Inc();
			RedisClient client = _redisClientQueue.Dequeue();
			try
			{
				client.Db = Db;
				return client.LockTake(key, value, expiry);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				_currentQueryCount.Dec();
			}
		}

		public int DbSize
		{
			get
			{
				_currentQueryCount.Inc();
				RedisClient client = _redisClientQueue.Dequeue();
				try
				{
					client.Db = Db;
					return client.DbSize;
				}
				finally
				{
					_redisClientQueue.Enqueue(client);
					_currentQueryCount.Dec();
				}
			}
		}

		public void Save()
		{
			_currentQueryCount.Inc();
			RedisClient client = _redisClientQueue.Dequeue();
			try
			{
				client.Db = Db;
				client.Save();
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				_currentQueryCount.Dec();
			}
		}

		public void BackgroundSave()
		{
			_currentQueryCount.Inc();
			RedisClient client = _redisClientQueue.Dequeue();
			try
			{
				client.Db = Db;
				client.BackgroundSave();
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				_currentQueryCount.Dec();
			}
		}

		public void FlushAll()
		{
			_currentQueryCount.Inc();
			RedisClient client = _redisClientQueue.Dequeue();
			try
			{
				client.Db = Db;
				client.FlushAll();
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				_currentQueryCount.Dec();
			}
		}

		public void FlushDb()
		{
			_currentQueryCount.Inc();
			RedisClient client = _redisClientQueue.Dequeue();
			try
			{
				client.Db = Db;
				client.FlushDb();
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				_currentQueryCount.Dec();
			}
		}

		public DateTime LastSave
		{
			get
			{
				_currentQueryCount.Inc();
				RedisClient client = _redisClientQueue.Dequeue();
				try
				{
					client.Db = Db;
					return client.LastSave;
				}
				finally
				{
					_redisClientQueue.Enqueue(client);
					_currentQueryCount.Dec();
				}
			}
		}

		public bool LockRelease(string key, int value)
		{
			_currentQueryCount.Inc();
			RedisClient client = _redisClientQueue.Dequeue();
			try
			{
				client.Db = Db;
				return client.LockRelease(key);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				_currentQueryCount.Dec();
			}
		}

		public KeyType TypeOf(string key)
		{
			_currentQueryCount.Inc();
			RedisClient client = _redisClientQueue.Dequeue();
			try
			{
				client.Db = Db;
				return client.TypeOf(key);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				_currentQueryCount.Dec();
			}
		}

		#region Set

		//public int SetAdd(string key, string[] values)
		//{
		//	if (string.IsNullOrEmpty(key))
		//	{
		//		throw new ArgumentException("Key is null or empty");
		//	}
		//	if (values == null || values.Length == 0)
		//	{
		//		throw new ArgumentException("Value is null or empty");
		//	}

		//	return SendExpectInt("SADD", key, values);
		//}

		public int SetAdd(string key, string value)
		{
			_currentQueryCount.Inc();
			RedisClient client = _redisClientQueue.Dequeue();
			try
			{
				client.Db = Db;
				return client.SetAdd(key, value);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				_currentQueryCount.Dec();
			}
		}

		public bool SetContains(string key, string value)
		{
			_currentQueryCount.Inc();
			RedisClient client = _redisClientQueue.Dequeue();
			try
			{
				client.Db = Db;
				return client.SetContains(key, value);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				_currentQueryCount.Dec();
			}
		}

		public long SetLength(string key)
		{
			_currentQueryCount.Inc();
			RedisClient client = _redisClientQueue.Dequeue();
			try
			{
				client.Db = Db;
				return client.SetLength(key);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				_currentQueryCount.Dec();
			}
		}

		public List<string> SetMembers(string key)
		{
			_currentQueryCount.Inc();
			RedisClient client = _redisClientQueue.Dequeue();
			try
			{
				client.Db = Db;
				return client.SetMembers(key);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				_currentQueryCount.Dec();
			}
		}

		public string SetPop(string key)
		{
			_currentQueryCount.Inc();
			RedisClient client = _redisClientQueue.Dequeue();
			try
			{
				client.Db = Db;
				return client.SetPop(key);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				_currentQueryCount.Dec();
			}
		}

		public bool SetRemove(string key, string value)
		{
			_currentQueryCount.Inc();
			RedisClient client = _redisClientQueue.Dequeue();
			try
			{
				client.Db = Db;
				return client.SetRemove(key, value);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				_currentQueryCount.Dec();
			}
		}

		#endregion

		#region Hash

		public bool HashExists(string key, string field)
		{
			_currentQueryCount.Inc();
			RedisClient client = _redisClientQueue.Dequeue();
			try
			{
				client.Db = Db;
				return client.HashExists(key, field);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				_currentQueryCount.Dec();
			}
		}

		public void HashDelete(string key, string field)
		{
			_currentQueryCount.Inc();
			RedisClient client = _redisClientQueue.Dequeue();
			try
			{
				client.Db = Db;
				client.HashDelete(key, field);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				_currentQueryCount.Dec();
			}
		}

		public KeyValuePair<string, string>[] HashGetAll(string key)
		{
			_currentQueryCount.Inc();
			RedisClient client = _redisClientQueue.Dequeue();
			try
			{
				client.Db = Db;
				return client.HashGetAll(key);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				_currentQueryCount.Dec();
			}
		}

		public string HashGet(string key, string field)
		{
			_currentQueryCount.Inc();
			RedisClient client = _redisClientQueue.Dequeue();
			try
			{
				client.Db = Db;
				return client.HashGet(key, field);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				_currentQueryCount.Dec();
			}
		}

		public long HashLength(string key)
		{
			_currentQueryCount.Inc();
			RedisClient client = _redisClientQueue.Dequeue();
			try
			{
				client.Db = Db;
				return client.HashLength(key);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				_currentQueryCount.Dec();
			}
		}

		public void HashSet(string set, string field, string value)
		{
			_currentQueryCount.Inc();
			RedisClient client = _redisClientQueue.Dequeue();
			try
			{
				client.Db = Db;
				client.HashSet(set, field, value);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				_currentQueryCount.Dec();
			}
		}

		#endregion

		#region SortedSet

		public string[] SortedSetRangeByRank(string key, int start, int stop)
		{
			_currentQueryCount.Inc();
			RedisClient client = _redisClientQueue.Dequeue();
			try
			{
				client.Db = Db;
				return client.SortedSetRangeByRank(key, start, stop);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				_currentQueryCount.Dec();
			}
		}

		public void SortedSetRemove(string key, string value)
		{
			_currentQueryCount.Inc();
			RedisClient client = _redisClientQueue.Dequeue();
			try
			{
				client.Db = Db;
				client.SortedSetRemove(key, value);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				_currentQueryCount.Dec();
			}
		}

		public bool SortedSetAdd(string key, string value, long score)
		{
			_currentQueryCount.Inc();
			RedisClient client = _redisClientQueue.Dequeue();
			try
			{
				client.Db = Db;
				return client.SortedSetAdd(key, value, score);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				_currentQueryCount.Dec();
			}
		}

		public long SortedSetLength(string key)
		{
			_currentQueryCount.Inc();
			RedisClient client = _redisClientQueue.Dequeue();
			try
			{
				client.Db = Db;
				return client.SortedSetLength(key);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				_currentQueryCount.Dec();
			}
		}

		#endregion

		#region Subscribe

		public bool Publish(string chanel, string message)
		{
			_currentQueryCount.Inc();
			RedisClient client = _redisClientQueue.Dequeue();
			try
			{
				return client.Publish(chanel, message);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				_currentQueryCount.Dec();
			}
		}

		public void Subscribe(string chanel, Action<string, string> action)
		{
			var redis = new RedisClient(Host, Port)
			{
				Password = Password,
				SendTimeout = SendTimeout,
				RetryCount = RetryCount,
				RetryTimeout = RetryTimeout
			};
			redis.Subscribe(chanel, action);
		}

		#endregion

		#region List commands

		public void ListLeftPush(string key, string value)
		{
			_currentQueryCount.Inc();
			RedisClient client = _redisClientQueue.Dequeue();
			try
			{
				client.Db = Db;
				client.ListLeftPush(key, value);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				_currentQueryCount.Dec();
			}
		}

		public void ListRightPush(string key, dynamic value)
		{
			_currentQueryCount.Inc();
			RedisClient client = _redisClientQueue.Dequeue();
			try
			{
				client.Db = Db;
				client.ListRightPush(key, value);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				_currentQueryCount.Dec();
			}
		}

		public int ListLength(string key)
		{
			_currentQueryCount.Inc();
			RedisClient client = _redisClientQueue.Dequeue();
			try
			{
				client.Db = Db;
				return client.ListLength(key);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				_currentQueryCount.Dec();
			}
		}

		public string ListLeftPop(string key)
		{
			_currentQueryCount.Inc();
			RedisClient client = _redisClientQueue.Dequeue();
			try
			{
				client.Db = Db;
				return client.ListLeftPop(key);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				_currentQueryCount.Dec();
			}
		}

		public string ListRightPop(string key)
		{
			_currentQueryCount.Inc();
			RedisClient client = _redisClientQueue.Dequeue();
			try
			{
				client.Db = Db;
				return client.ListRightPop(key);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				_currentQueryCount.Dec();
			}
		}

		#endregion

		public void Dispose()
		{
			_isDisposing = true;
		}
	}

	public enum KeyType
	{
		None, String, List, Set
	}

	internal class RedisClient : IDisposable
	{
		public string Host { get; }
		public int Port { get; }
		public int RetryTimeout { get; set; }
		public int RetryCount { get; set; }
		public int SendTimeout { get; set; }
		public string Password { get; set; }
		private int _db;
		private Socket _socket;
		private BufferedStream _bstream;
		private readonly byte[] _endData = { (byte)'\r', (byte)'\n' };
		private const long UnixEpoch = 621355968000000000L;
		private bool _isClosing;

		public RedisClient(string host, int port)
		{
			if (host == null)
			{
				throw new ArgumentException("Host is null");
			}

			if (port <= 0 || port > 65535)
			{
				throw new ArgumentException("Port range: 1~65535");
			}

			Host = host;
			Port = port;
			SendTimeout = -1;
		}

		public RedisClient(string host) : this(host, 6379)
		{
		}

		public RedisClient() : this("localhost", 6379)
		{
		}

		public int Db
		{
			get
			{
				return _db;
			}

			set
			{
				if (value > 0 && _db != value)
				{
					_db = value;
					SendExpectSuccess("SELECT", _db);
				}
			}
		}

		#region base

		public bool KeyDelete(string key)
		{
			lock (this)
			{
				return SendExpectInt("DEL", key) > 0;
			}
		}

		public bool ContainsKey(string key)
		{
			lock (this)
			{
				if (key == null)
				{
					throw new ArgumentException("key");
				}

				return SendExpectInt("EXISTS", key) == 1;
			}
		}

		public bool LockTake(string key, string value, TimeSpan expiry)
		{
			lock (this)
			{
				return StringSet(key, value, expiry);
			}
		}

		public KeyType TypeOf(string key)
		{
			lock (this)
			{
				if (key == null)
				{
					throw new ArgumentException("key");
				}
				switch (SendExpectString("TYPE", key))
				{
					case "none":
						return KeyType.None;
					case "string":
						return KeyType.String;
					case "set":
						return KeyType.Set;
					case "list":
						return KeyType.List;
				}
				throw new ResponseException("Invalid value");
			}
		}

		//public bool Rename(string oldKeyname, string newKeyname)
		//{
		//	if (oldKeyname == null)
		//		throw new ArgumentException("oldKeyname");
		//	if (newKeyname == null)
		//		throw new ArgumentException("newKeyname");
		//	return SendGetString("RENAME", oldKeyname, newKeyname)[0] == '+';
		//}

		public int DbSize
		{
			get
			{
				lock (this)
				{
					return SendExpectInt("DBSIZE");
				}
			}
		}

		public void Save()
		{
			lock (this)
			{
				SendExpectSuccess("SAVE");
			}
		}

		public void BackgroundSave()
		{
			lock (this)
			{
				SendExpectSuccess("BGSAVE");
			}
		}

		public void FlushAll()
		{
			lock (this)
			{
				SendExpectSuccess("FLUSHALL");
			}
		}

		public void FlushDb()
		{
			lock (this)
			{
				SendExpectSuccess("FLUSHDB");
			}
		}

		public DateTime LastSave
		{
			get
			{
				lock (this)
				{
					int t = SendExpectInt("LASTSAVE");
					return new DateTime(UnixEpoch) + TimeSpan.FromSeconds(t);
				}
			}
		}

		public bool LockRelease(string key)
		{
			return KeyDelete(key);
		}

		#endregion

		#region Set

		//public int SetAdd(string key, string[] values)
		//{
		//	if (string.IsNullOrEmpty(key))
		//	{
		//		throw new ArgumentException("Key is null or empty");
		//	}
		//	if (values == null || values.Length == 0)
		//	{
		//		throw new ArgumentException("Value is null or empty");
		//	}

		//	return SendExpectInt("SADD", key, values);
		//}

		public int SetAdd(string key, string value)
		{
			lock (this)
			{
				if (string.IsNullOrEmpty(key))
				{
					throw new ArgumentException("Key is null or empty");
				}
				if (value == null)
				{
					throw new ArgumentException("Value is null");
				}
				var bytes = Encoding.UTF8.GetBytes(value);

				if (bytes.Length > 1073741824)
				{
					throw new ArgumentException("value exceeds 1G");
				}

				return SendDataExpectInt(bytes, "SADD", key);
			}
		}

		public bool SetContains(string key, string value)
		{
			lock (this)
			{
				return SendExpectInt("SISMEMBER", key, value) > 0;
			}
		}

		public long SetLength(string key)
		{
			lock (this)
			{
				return SendExpectLong("SCARD", key);
			}
		}

		public List<string> SetMembers(string key)
		{
			lock (this)
			{
				SendCommand("SMEMBERS", key);
				byte[][] results = ReadData();
				List<string> list = new List<string>();

				foreach (var item in results)
				{
					list.Add(Encoding.UTF8.GetString(item));
				}
				return list;
			}
		}

		public string SetPop(string key)
		{
			lock (this)
			{
				SendCommand("SPOP", key);

				var results = ReadData();
				return results == null ? null : Encoding.UTF8.GetString(results);
			}
		}

		public bool SetRemove(string key, string value)
		{
			lock (this)
			{
				return SendExpectInt("SREM", key, value) > 0;
			}
		}

		#endregion

		#region Hash

		public bool HashExists(string key, string field)
		{
			lock (this)
			{
				return SendExpectInt("HEXISTS", key, field) > 0;
			}
		}

		public void HashDelete(string key, string field)
		{
			lock (this)
			{
				SendExpectSuccess("HDEL", key, field);
			}
		}

		public KeyValuePair<string, string>[] HashGetAll(string key)
		{
			lock (this)
			{
				Dictionary<string, string> dic = new Dictionary<string, string>();
				SendCommand("HGETALL", key);
				var results = ReadData();
				if (results == null)
				{
					return new KeyValuePair<string, string>[0];
				}
				for (int i = 0; i < results.Length;)
				{
					string k = Encoding.UTF8.GetString(results[i]);
					string value = Encoding.UTF8.GetString(results[i + 1]);
					dic.Add(k, value);
					i = i + 2;
				}
				return dic.ToArray();
			}
		}

		public long HashLength(string key)
		{
			lock (this)
			{
				return SendExpectLong("HLEN", key);
			}
		}

		public void HashSet(string set, string field, string value)
		{
			lock (this)
			{
				if (string.IsNullOrEmpty(set))
				{
					throw new ArgumentException("Key is null or empty.");
				}
				if (string.IsNullOrEmpty(field))
				{
					throw new ArgumentException("Field is null or empty.");
				}

				SendExpectSuccess("HSET", set, field, value);
			}
		}

		#endregion

		#region SortedSet

		public string[] SortedSetRangeByRank(string key, int start, int stop)
		{
			lock (this)
			{
				return SendExpectStringArray("ZRANGE", key, start, stop);
			}
		}

		public void SortedSetRemove(string key, string value)
		{
			lock (this)
			{
				SendExpectSuccess("ZREM", key, value);
			}
		}

		public bool SortedSetAdd(string key, string value, long score)
		{
			lock (this)
			{
				return SendExpectInt("ZADD", key, score, value) > 0;
			}
		}

		public long SortedSetLength(string key)
		{
			lock (this)
			{
				return SendExpectLong("ZCARD", key);
			}
		}

		public string HashGet(string key, string field)
		{
			lock (this)
			{
				SendCommand("HGET", key, field);
				var results = ReadData();
				return results == null ? null : Encoding.UTF8.GetString(results);
			}
		}

		#endregion

		#region Subscribe

		public bool Publish(string chanel, string message)
		{
			lock (this)
			{
				return SendExpectInt("PUBLISH", chanel, message) > 0;
			}
		}

		public void Subscribe(string chanel, Action<string, string> action)
		{
			SendExpectDataArray("SUBSCRIBE", chanel);
			Task.Factory.StartNew(() =>
			{
				while (true)
				{
					if (_isClosing)
					{
						break;
					}

					dynamic r = ReadData();

					action(Encoding.UTF8.GetString(r[1]), Encoding.UTF8.GetString(r[2]));
				}
			});
		}

		#endregion

		#region List commands

		public void ListLeftPush(string key, string value)
		{
			lock (this)
			{
				SendDataExpectSuccess(Encoding.UTF8.GetBytes(value), "LPUSH", key);
			}
		}

		public void ListRightPush(string key, dynamic value)
		{
			lock (this)
			{
				SendDataExpectSuccess(Encoding.UTF8.GetBytes(value.ToString()), "RPUSH", key);
			}
		}

		public int ListLength(string key)
		{
			lock (this)
			{
				return SendExpectInt("LLEN", key);
			}
		}

		public string ListLeftPop(string key)
		{
			lock (this)
			{
				SendCommand("LPOP", key);
				var result = ReadData();
				return result == null ? null : Encoding.UTF8.GetString(result);
			}
		}

		public string ListRightPop(string key)
		{
			lock (this)
			{
				SendCommand("RPOP", key);
				var result = ReadData();
				return result == null ? null : Encoding.UTF8.GetString(result);
			}
		}

		#endregion

		private byte[][] Sort(SortOptions options)
		{
			return Sort(options.Key, options.StoreInKey, options.ToArgs());
		}

		private byte[][] Sort(string key, string destination, params object[] options)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentException("key");
			}

			int offset = string.IsNullOrEmpty(destination) ? 1 : 3;
			object[] args = new object[offset + options.Length];

			args[0] = key;
			Array.Copy(options, 0, args, offset, options.Length);
			if (offset == 1)
			{
				return SendExpectDataArray("SORT", args);
			}
			else {
				args[1] = "STORE";
				args[2] = destination;
				int n = SendExpectInt("SORT", args);
				return new byte[n][];
			}
		}

		#region utils

		private bool StringSet(string key, string value, TimeSpan? expiry = null)
		{
			bool status = SendCommand("SET", key, value);
			if (expiry != null && status)
			{
				long milliseconds = expiry.Value.Ticks / TimeSpan.TicksPerMillisecond;
				return SendCommand("PEXPIRE ", key, milliseconds);
			}
			return false;
		}

		private string ReadLine()
		{
			StringBuilder sb = new StringBuilder();
			int c;

			while ((c = _bstream.ReadByte()) != -1)
			{
				if (c == '\r')
					continue;
				if (c == '\n')
					break;
				sb.Append((char)c);
			}
			return sb.ToString();
		}

		private static IPHostEntry ParseIpAddress(string hostname)
		{
			IPHostEntry ipHe = null;

			IPAddress addr;
			if (IPAddress.TryParse(hostname, out addr))
			{
				ipHe = new IPHostEntry();
				ipHe.AddressList = new IPAddress[1];
				ipHe.AddressList[0] = addr;
			}

			return ipHe;
		}

		private static IPHostEntry GetHostEntry(string hostname)
		{
			IPHostEntry ipHe = ParseIpAddress(hostname);
			if (ipHe != null) return ipHe;
			return Dns.GetHostEntryAsync(hostname).Result;
		}

		private void Connect()
		{
			_socket = CreateSocket();
			if (_socket == null)
			{
				throw new Exception("Can't connect to server.");
			}
			_bstream = new BufferedStream(new NetworkStream(_socket), 16 * 1024);

			if (Password != null)
			{
				SendExpectSuccess("AUTH", Password);
			}
		}

		private Socket CreateSocket()
		{
			Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			socket.NoDelay = true;
			socket.SendTimeout = SendTimeout;

			IPHostEntry ipHe = GetHostEntry(Host);

			foreach (var address in ipHe.AddressList)
			{
				try
				{
					socket.Connect(new IPEndPoint(address, Port));
					break;
				}
				catch
				{
					// ignored
				}
			}

			if (!socket.Connected)
			{
				socket.Dispose();
				socket = null;
			}
			return socket;
		}

#if !RELEASE
		void Log(string id, string message)
		{
			Console.WriteLine(id + ": " + message.Trim().Replace("\r\n", " "));
		}
#endif

		private string[] SendExpectStringArray(string cmd, params object[] args)
		{
			byte[][] reply = SendExpectDataArray(cmd, args);
			string[] keys = new string[reply.Length];
			for (int i = 0; i < reply.Length; i++)
			{
				keys[i] = Encoding.UTF8.GetString(reply[i]);
			}
			return keys;
		}

		private byte[][] SendExpectDataArray(string cmd, params object[] args)
		{
			if (!SendCommand(cmd, args))
			{
				throw new Exception("Unable to connect");
			}

			int c = _bstream.ReadByte();
			string s = ReadLine();

			if (c == -1)
			{
				throw new ResponseException("No more data");
			}

			Log("S", (char)c + s);
			if (c == '-')
			{
				throw new ResponseException(s.StartsWith("ERR ") ? s.Substring(4) : s);
			}
			if (c == '*')
			{
				int count;
				if (int.TryParse(s, out count))
				{
					byte[][] result = new byte[count][];

					for (int i = 0; i < count; i++)
						result[i] = ReadData();

					return result;
				}
			}
			throw new ResponseException("Unknown reply on multi-request: " + c + s);
		}

		private void SendExpectSuccess(string cmd, params object[] args)
		{
			if (!SendCommand(cmd, args))
			{
				throw new Exception("Unable to connect");
			}

			int c = _bstream.ReadByte();
			string s = ReadLine();

			if (c == -1)
			{
				throw new ResponseException("No more data");
			}
			Log("S", (char)c + s);
			if (c == '-')
			{
				throw new ResponseException(s.StartsWith("ERR ") ? s.Substring(4) : s);
			}
		}

		private class RedisResult
		{
			public int Flag { get; set; }
			public dynamic Result { get; set; }
			public Exception Exeption { get; set; }
		}

		private int SendDataExpectInt(byte[] data, string cmd, params object[] args)
		{
			if (!SendDataCommand(data, cmd, args))
				throw new Exception("Unable to connect");

			int c = _bstream.ReadByte();
			string s = ReadLine();

			if (c == -1)
			{
				throw new ResponseException("No more data");
			}

			if (c == ':')
			{
				int i;
				if (int.TryParse(s, out i))
					return i;
			}

			throw new ResponseException("Unknown reply on integer request: " + c + s);
		}

		private void SendDataExpectSuccess(byte[] data, string cmd, params object[] args)
		{
			if (!SendDataCommand(data, cmd, args))
				throw new Exception("Unable to connect");

			int c = _bstream.ReadByte();
			string s = ReadLine();

			if (c == -1)
			{
				throw new ResponseException("No more data");
			}
			Log("S", (char)c + s);
			if (c == '-')
			{
				throw new ResponseException(s.StartsWith("ERR ") ? s.Substring(4) : s);
			}
		}

		private int SendExpectInt(string cmd, params object[] args)
		{
			if (!SendCommand(cmd, args))
				throw new Exception("Unable to connect");

			int c = _bstream.ReadByte();
			string s = ReadLine();

			if (c == -1)
			{
				throw new ResponseException("No more data");
			}

			Log("S", (char)c + s);
			if (c == '-')
				throw new ResponseException(s.StartsWith("ERR ") ? s.Substring(4) : s);
			if (c == ':')
			{
				int i;
				if (int.TryParse(s, out i))
					return i;
			}
			throw new ResponseException("Unknown reply on integer request: " + c + s);
		}

		private long SendExpectLong(string cmd, params object[] args)
		{
			if (!SendCommand(cmd, args))
			{
				throw new Exception("Unable to connect");
			}

			int c = _bstream.ReadByte();
			string s = ReadLine();
			if (c == -1)
			{
				throw new ResponseException("No more data");
			}

			Log("S", (char)c + s);
			if (c == '-')
			{
				throw new ResponseException(s.StartsWith("ERR ") ? s.Substring(4) : s);
			}
			if (c == ':')
			{
				long i;
				if (long.TryParse(s, out i))
					return i;
			}
			throw new ResponseException("Unknown reply on integer request: " + c + s);
		}

		private string SendExpectString(string cmd, params object[] args)
		{
			if (!SendCommand(cmd, args))
			{
				throw new Exception("Unable to connect");
			}

			int c = _bstream.ReadByte();
			string s = ReadLine();

			if (c == -1)
			{
				throw new ResponseException("No more data");
			}

			Log("S", (char)c + s);
			if (c == '-')
			{
				throw new ResponseException(s.StartsWith("ERR ") ? s.Substring(4) : s);
			}
			if (c == '+')
			{
				return s;
			}

			throw new ResponseException("Unknown reply on integer request: " + c + s);
		}

		private dynamic ReadData()
		{
			string s = ReadLine();

			Log("S", s);
			if (s.Length == 0)
				throw new ResponseException("Zero length respose");

			char c = s[0];
			if (c == '-')
				throw new ResponseException(s.StartsWith("-ERR ") ? s.Substring(5) : s.Substring(1));

			if (c == ':')
			{
				int i;
				if (int.TryParse(s.Substring(1), out i))
				{
					return new[] { (byte)i };
				}
			}

			if (c == '$')
			{
				if (s == "$-1")
					return null;
				int n;

				if (Int32.TryParse(s.Substring(1), out n))
				{
					byte[] retbuf = new byte[n];

					int bytesRead = 0;
					do
					{
						int read = _bstream.Read(retbuf, bytesRead, n - bytesRead);
						if (read < 1)
							throw new ResponseException("Invalid termination mid stream");
						bytesRead += read;
					}
					while (bytesRead < n);
					if (_bstream.ReadByte() != '\r' || _bstream.ReadByte() != '\n')
						throw new ResponseException("Invalid termination");
					return retbuf;
				}
				throw new ResponseException("Invalid length");
			}

			if (c == '*')
			{
				int count;
				if (int.TryParse(s.Substring(1), out count))
				{
					byte[][] result = new byte[count][];

					for (int i = 0; i < count; i++)
						result[i] = ReadData();

					return result;
				}
			}

			throw new ResponseException("Unexpected reply: ");
		}

		private bool SendDataCommand(byte[] data, string cmd, params object[] args)
		{
			string resp = "*" + (1 + args.Length + 1) + "\r\n";
			resp += "$" + cmd.Length + "\r\n" + cmd + "\r\n";
			foreach (object arg in args)
			{
				string argStr = arg.ToString();
				int argStrLength = Encoding.UTF8.GetByteCount(argStr);
				resp += "$" + argStrLength + "\r\n" + argStr + "\r\n";
			}
			if (data != null)
			{
				resp += "$" + data.Length + "\r\n";
			}
			return SendDataResp(data, resp);
		}

		private bool SendDataResp(byte[] data, string resp)
		{
			if (_socket == null)
			{
				Connect();
			}

			if (_socket == null)
			{
				return false;
			}

			byte[] r = Encoding.UTF8.GetBytes(resp);
			try
			{
				Log("C", resp);
				_socket.Send(r);
				if (data != null)
				{
					_socket.Send(data);
					_socket.Send(_endData);
				}
			}
			catch (SocketException)
			{
				// timeout;
				_socket.Dispose();
				return false;
			}
			return true;
		}

		private bool SendCommand(string cmd, params object[] args)
		{
			if (_socket == null)
			{
				Connect();
			}

			if (_socket == null)
			{
				return false;
			}

			string resp = "*" + (1 + args.Length) + "\r\n";
			resp += "$" + cmd.Length + "\r\n" + cmd + "\r\n";
			foreach (object arg in args)
			{
				if (arg == null)
				{
					continue;
				}
				string argStr = arg.ToString();
				int argStrLength = Encoding.UTF8.GetByteCount(argStr);
				resp += "$" + argStrLength + "\r\n" + argStr + "\r\n";
			}

			byte[] r = Encoding.UTF8.GetBytes(resp);
			try
			{
				_socket.Send(r);

				//RedisResult result = GetResult();
			}
			catch (SocketException)
			{
				// timeout;
				_socket.Dispose();
				_socket = null;
				return false;
			}

			return true;
		}

		//private RedisResult GetResult()
		//{
			
		//}

		#endregion

		public void Dispose()
		{
			_isClosing = true;

			try
			{
				SendExpectSuccess("QUIT");

				_socket.Dispose();
				_socket = null;
			}
			catch
			{
				// ignored
			}
		}

		public class ResponseException : Exception
		{
			public ResponseException(string code) : base("Response error")
			{
				Code = code;
			}

			public string Code { get; private set; }
		}
	}

	public class SortOptions
	{
		public string Key { get; set; }
		public bool Descending { get; set; }
		public bool Lexographically { get; set; }
		public Int32 LowerLimit { get; set; }
		public Int32 UpperLimit { get; set; }
		public string By { get; set; }
		public string StoreInKey { get; set; }
		public string Get { get; set; }

		public object[] ToArgs()
		{
			System.Collections.ArrayList args = new System.Collections.ArrayList();

			if (LowerLimit != 0 || UpperLimit != 0)
			{
				args.Add("LIMIT");
				args.Add(LowerLimit);
				args.Add(UpperLimit);
			}
			if (Lexographically)
				args.Add("ALPHA");
			if (!string.IsNullOrEmpty(By))
			{
				args.Add("BY");
				args.Add(By);
			}
			if (!string.IsNullOrEmpty(Get))
			{
				args.Add("GET");
				args.Add(Get);
			}
			return args.ToArray();
		}
	}
}



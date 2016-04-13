using System;
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
		private readonly AtomicInteger _clientCount = new AtomicInteger(0);
		public string Host { get; }
		public int Port { get; }
		public int RetryTimeout { get; set; }
		public int RetryCount { get; set; }
		public int SendTimeout { get; set; }
		public string Password { get; }
		public int MaxThreadNum { get; }
		public int Db { get; set; }
		private bool _isDisposing;

		public RedisServer(string host, int port, string pass, int maxThreadNum = 10)
		{
            Console.WriteLine($"Redis Server: {host} {port}");
			Host = host;
			Port = port;
			Password = pass;
			if (maxThreadNum > 150)
			{
				throw new Exception("We strongly suggest you don't open too much thread.");
			}
			MaxThreadNum = maxThreadNum;

			_redisClientQueue = new BlockingQueue<RedisClient>(maxThreadNum);
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

		public bool KeyDelete(string key)
		{
			
			RedisClient client = CreateRedisClient();
			try
			{
				client.Db = Db;
				return client.KeyDelete(key);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				
			}
		}

		private RedisClient CreateRedisClient()
		{
            if(_clientCount.Value<MaxThreadNum && _redisClientQueue.Count==0)
            {
			    return new RedisClient(Host, Port) { Password = Password, RetryCount = RetryCount, RetryTimeout = RetryTimeout };
            }
            else
            {
               return  _redisClientQueue.Dequeue();
            }
		}

		public bool ContainsKey(string key)
		{
			
			RedisClient client = CreateRedisClient();
			try
			{
				client.Db = Db;
				return client.ContainsKey(key);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);				
			}
		}

		public bool LockTake(string key, string value, TimeSpan expiry)
		{
			
			RedisClient client = CreateRedisClient();
			try
			{
				client.Db = Db;
				return client.LockTake(key, value, expiry);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				
			}
		}

		public int DbSize
		{
			get
			{
				
				RedisClient client = CreateRedisClient();
				try
				{
					client.Db = Db;
					return client.DbSize;
				}
				finally
				{
					_redisClientQueue.Enqueue(client);
					
				}
			}
		}

		public void Save()
		{
			
			RedisClient client = CreateRedisClient();
			try
			{
				client.Db = Db;
				client.Save();
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				
			}
		}

		public void BackgroundSave()
		{
			
			RedisClient client = CreateRedisClient();
			try
			{
				client.Db = Db;
				client.BackgroundSave();
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				
			}
		}

		public void FlushAll()
		{
			
			RedisClient client = CreateRedisClient();
			try
			{
				client.Db = Db;
				client.FlushAll();
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				
			}
		}

		public void FlushDb()
		{
			
			RedisClient client = CreateRedisClient();
			try
			{
				client.Db = Db;
				client.FlushDb();
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				
			}
		}

		public DateTime LastSave
		{
			get
			{
				
				RedisClient client = CreateRedisClient();
				try
				{
					client.Db = Db;
					return client.LastSave;
				}
				finally
				{
					_redisClientQueue.Enqueue(client);
					
				}
			}
		}

		public bool LockRelease(string key, int value)
		{
			
			RedisClient client = CreateRedisClient();
			try
			{
				client.Db = Db;
				return client.LockRelease(key);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				
			}
		}

		public KeyType TypeOf(string key)
		{
			
			RedisClient client = CreateRedisClient();
			try
			{
				client.Db = Db;
				return client.TypeOf(key);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				
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
			
			RedisClient client = CreateRedisClient();
			try
			{
				client.Db = Db;
				return client.SetAdd(key, value);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				
			}
		}

		public bool SetContains(string key, string value)
		{
			
			RedisClient client = CreateRedisClient();
			try
			{
				client.Db = Db;
				return client.SetContains(key, value);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				
			}
		}

		public long SetLength(string key)
		{
			
			RedisClient client = CreateRedisClient();
			try
			{
				client.Db = Db;
				return client.SetLength(key);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				
			}
		}

		public List<string> SetMembers(string key)
		{
			
			RedisClient client = CreateRedisClient();
			try
			{
				client.Db = Db;
				return client.SetMembers(key);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				
			}
		}

		public string SetPop(string key)
		{
			
			RedisClient client = CreateRedisClient();
			try
			{
				client.Db = Db;
				return client.SetPop(key);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				
			}
		}

		public bool SetRemove(string key, string value)
		{
			
			RedisClient client = CreateRedisClient();
			try
			{
				client.Db = Db;
				return client.SetRemove(key, value);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				
			}
		}

		#endregion

		#region Hash

		public bool HashExists(string key, string field)
		{
			
			RedisClient client = CreateRedisClient();
			try
			{
				client.Db = Db;
				return client.HashExists(key, field);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				
			}
		}

		public void HashDelete(string key, string field)
		{
			
			RedisClient client = CreateRedisClient();
			try
			{
				client.Db = Db;
				client.HashDelete(key, field);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				
			}
		}

		public KeyValuePair<string, string>[] HashGetAll(string key)
		{
			
			RedisClient client = CreateRedisClient();
			try
			{
				client.Db = Db;
				return client.HashGetAll(key);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				
			}
		}

		public string HashGet(string key, string field)
		{
			
			RedisClient client = CreateRedisClient();
			try
			{
				client.Db = Db;
				return client.HashGet(key, field);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				
			}
		}

		public long HashLength(string key)
		{
			
			RedisClient client = CreateRedisClient();
			try
			{
				client.Db = Db;
				return client.HashLength(key);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				
			}
		}

		public void HashSet(string set, string field, string value)
		{
			
			RedisClient client = CreateRedisClient();
			try
			{
				client.Db = Db;
				client.HashSet(set, field, value);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				
			}
		}

		#endregion

		#region SortedSet

		public string[] SortedSetRangeByRank(string key, int start, int stop)
		{
			
			RedisClient client = CreateRedisClient();
			try
			{
				client.Db = Db;
				return client.SortedSetRangeByRank(key, start, stop);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				
			}
		}

		public void SortedSetRemove(string key, string value)
		{
			
			RedisClient client = CreateRedisClient();
			try
			{
				client.Db = Db;
				client.SortedSetRemove(key, value);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				
			}
		}

		public bool SortedSetAdd(string key, string value, long score)
		{
			
			RedisClient client = CreateRedisClient();
			try
			{
				client.Db = Db;
				return client.SortedSetAdd(key, value, score);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				
			}
		}

		public long SortedSetLength(string key)
		{
			
			RedisClient client = CreateRedisClient();
			try
			{
				client.Db = Db;
				return client.SortedSetLength(key);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				
			}
		}

		#endregion

		#region Subscribe

		public bool Publish(string chanel, string message)
		{
			
			RedisClient client = CreateRedisClient();
			try
			{
				return client.Publish(chanel, message);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				
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
			
			RedisClient client = CreateRedisClient();
			try
			{
				client.Db = Db;
				client.ListLeftPush(key, value);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				
			}
		}

		public void ListRightPush(string key, dynamic value)
		{
			
			RedisClient client = CreateRedisClient();
			try
			{
				client.Db = Db;
				client.ListRightPush(key, value);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				
			}
		}

		public int ListLength(string key)
		{
			
			RedisClient client = CreateRedisClient();
			try
			{
				client.Db = Db;
				return client.ListLength(key);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				
			}
		}

		public string ListLeftPop(string key)
		{
			
			RedisClient client = CreateRedisClient();
			try
			{
				client.Db = Db;
				return client.ListLeftPop(key);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);
				
			}
		}

		public string ListRightPop(string key)
		{
			
			RedisClient client = CreateRedisClient();
			try
			{
				client.Db = Db;
				return client.ListRightPop(key);
			}
			finally
			{
				_redisClientQueue.Enqueue(client);				
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
				long milliseconds = expiry.Ticks / TimeSpan.TicksPerMillisecond;
				var results = SendCommand("SET", key, value, "PX", milliseconds);
				if (results.Exeption != null)
				{
					throw results.Exeption;
				}
				return true;
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
				throw new Exception("Invalid value");
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
				var result = SendCommand("SMEMBERS", key);

				List<string> list = new List<string>();

				foreach (var item in result.Result)
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
				var result = SendCommand("SPOP", key);
				if (result.Exeption != null)
				{
					throw result.Exeption;
				}
				var results = result.Result;

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
				var result = SendCommand("HGETALL", key);
				if (result.Exeption != null)
				{
					throw result.Exeption;
				}
				var results = result.Result;
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
				var result = SendCommand("HGET", key, field);
				if (result.Exeption != null)
				{
					throw result.Exeption;
				}
				var results = result.Result;

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
			Task.Factory.StartNew(() =>
			{
				var results = SendCommand("SUBSCRIBE", chanel);
				if (results.Exeption != null)
				{
					throw results.Exeption;
				}

				while (true)
				{
					if (_isClosing)
					{
						break;
					}

					results = GetResult();
					if (results.Exeption != null)
					{
						throw results.Exeption;
					}
					action(Encoding.UTF8.GetString(results.Result[1]), Encoding.UTF8.GetString(results.Result[2]));
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
				var result = SendCommand("LPOP", key);
				if (result.Exeption != null)
				{
					throw result.Exeption;
				}

				return result.Result == null ? null : Encoding.UTF8.GetString(result.Result);
			}
		}

		public string ListRightPop(string key)
		{
			lock (this)
			{
				var results = SendCommand("RPOP", key);
				if (results.Exeption != null)
				{
					throw results.Exeption;
				}

				return results.Result == null ? null : Encoding.UTF8.GetString(results.Result);
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

			if (!string.IsNullOrEmpty(Password))
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
			var result = SendCommand(cmd, args);

			if (result.Exeption != null)
			{
				throw result.Exeption;
			}

			return result.Result;

			//int c = _bstream.ReadByte();
			//string s = ReadLine();

			//if (c == -1)
			//{
			//	throw new ResponseException("No more data");
			//}

			//Log("S", (char)c + s);
			//if (c == '-')
			//{
			//	throw new ResponseException(s.StartsWith("ERR ") ? s.Substring(4) : s);
			//}
			//if (c == '*')
			//{
			//	int count;
			//	if (int.TryParse(s, out count))
			//	{
			//		byte[][] result = new byte[count][];

			//		for (int i = 0; i < count; i++)
			//			result[i] = ReadData();

			//		return result;
			//	}
			//}
			//throw new ResponseException("Unknown reply on multi-request: " + c + s);
		}

		private void SendExpectSuccess(string cmd, params object[] args)
		{
			var result = SendCommand(cmd, args);

			if (result.Exeption != null)
			{
				throw result.Exeption;
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
			var result = SendDataCommand(data, cmd, args);

			if (result.Exeption != null)
			{
				throw result.Exeption;
			}
			else
			{
				return (result.Result is string) ? int.Parse(result.Result) : result.Result;
			}
		}

		private void SendDataExpectSuccess(byte[] data, string cmd, params object[] args)
		{
			var result = SendDataCommand(data, cmd, args);

			if (result.Exeption != null)
			{
				throw result.Exeption;
			}
		}

		private int SendExpectInt(string cmd, params object[] args)
		{
			var result = SendCommand(cmd, args);

			if (result.Exeption != null)
			{
				throw result.Exeption;
			}
			else
			{
				return (int)result.Result;
			}
		}

		private long SendExpectLong(string cmd, params object[] args)
		{
			var result = SendCommand(cmd, args);

			if (result.Exeption != null)
			{
				throw result.Exeption;
			}
			else
			{
				return (result.Result is string) ? long.Parse(result.Result) : result.Result;
			}
		}

		private string SendExpectString(string cmd, params object[] args)
		{
			var result = SendCommand(cmd, args);

			if (result.Exeption != null)
			{
				throw result.Exeption;
			}
			else
			{
				return result.Result;
			}
		}

		//private dynamic ReadData()
		//{
		//	string s = ReadLine();

		//	Log("S", s);
		//	if (s.Length == 0)
		//		throw new ResponseException("Zero length respose");

		//	char c = s[0];
		//	if (c == '-')
		//		throw new ResponseException(s.StartsWith("-ERR ") ? s.Substring(5) : s.Substring(1));

		//	if (c == ':')
		//	{
		//		int i;
		//		if (int.TryParse(s.Substring(1), out i))
		//		{
		//			return new[] { (byte)i };
		//		}
		//	}

		//	if (c == '$')
		//	{
		//		if (s == "$-1")
		//			return null;
		//		int n;

		//		if (Int32.TryParse(s.Substring(1), out n))
		//		{
		//			byte[] retbuf = new byte[n];

		//			int bytesRead = 0;
		//			do
		//			{
		//				int read = _bstream.Read(retbuf, bytesRead, n - bytesRead);
		//				if (read < 1)
		//					throw new ResponseException("Invalid termination mid stream");
		//				bytesRead += read;
		//			}
		//			while (bytesRead < n);
		//			if (_bstream.ReadByte() != '\r' || _bstream.ReadByte() != '\n')
		//				throw new ResponseException("Invalid termination");
		//			return retbuf;
		//		}
		//		throw new ResponseException("Invalid length");
		//	}

		//	if (c == '*')
		//	{
		//		int count;
		//		if (int.TryParse(s.Substring(1), out count))
		//		{
		//			byte[][] result = new byte[count][];

		//			for (int i = 0; i < count; i++)
		//				result[i] = ReadData();

		//			return result;
		//		}
		//	}

		//	throw new ResponseException("Unexpected reply: ");
		//}

		private RedisResult SendDataCommand(byte[] data, string cmd, params object[] args)
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

		private RedisResult SendDataResp(byte[] data, string resp)
		{
			if (_socket == null)
			{
				Connect();
			}

			if (_socket == null)
			{
				return new RedisResult() { Exeption = new Exception("Socket is null.") };
			}

			byte[] r = Encoding.UTF8.GetBytes(resp);
			try
			{
				_socket.Send(r);
				_socket.Send(data);
				_socket.Send(_endData);

				return GetResult();
			}
			catch (SocketException s)
			{
				// timeout;
				_socket.Dispose();
				return new RedisResult() { Exeption = s };
			}
		}

		private RedisResult SendCommand(string cmd, params object[] args)
		{
			if (_socket == null)
			{
				Connect();
			}

			if (_socket == null)
			{
				return new RedisResult() { Exeption = new Exception("Socket is null.") };
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

				return GetResult();
			}
			catch (SocketException s)
			{
				_socket.Dispose();
				_socket = null;
				return new RedisResult() { Exeption = s };
			}
		}

		private RedisResult GetResult()
		{
			RedisResult redisResult = new RedisResult();
			redisResult.Flag = _bstream.ReadByte();

			string msg = ReadLine();

			if (redisResult.Flag == '-')
			{
				redisResult.Exeption = new Exception(msg.StartsWith("-ERR ") ? msg.Substring(5) : msg.Substring(1));
				return redisResult;
			}

			if (redisResult.Flag == ':')
			{
				int i;
				if (int.TryParse(msg, out i))
				{
					redisResult.Result = i;
					return redisResult;
				}
			}

			if (redisResult.Flag == '$')
			{
				if (msg == "-1")
				{
					return redisResult;
				}
				int n;

				if (int.TryParse(msg, out n))
				{
					byte[] retbuf = new byte[n];

					int bytesRead = 0;
					do
					{
						int read = _bstream.Read(retbuf, bytesRead, n - bytesRead);
						if (read < 1)
						{
							redisResult.Exeption = new Exception("Invalid termination mid stream");
							return redisResult;
						}
						bytesRead += read;
					}
					while (bytesRead < n);
					if (_bstream.ReadByte() != '\r' || _bstream.ReadByte() != '\n')
					{
						redisResult.Exeption = new Exception("Invalid termination");
						return redisResult;
					}
					redisResult.Result = retbuf;
					return redisResult;
				}

				redisResult.Exeption = new Exception("Invalid length");
				return redisResult;
			}

			if (redisResult.Flag == '*')
			{
				int count;
				if (int.TryParse(msg, out count))
				{
					byte[][] result = new byte[count][];

					for (int i = 0; i < count; i++)
					{
						var tmp = GetResult();
						if (tmp.Exeption == null)
						{
							result[i] = (tmp.Result is byte[]) ? tmp.Result : new [] { (byte)tmp.Result };
						}
						else
						{
							redisResult.Exeption = tmp.Exeption;
							return redisResult;
						}
					}

					redisResult.Result = result;
					return redisResult;
				}
			}

			if (redisResult.Flag == '+')
			{
				redisResult.Result = msg;
				return redisResult;
			}
			redisResult.Exeption = new Exception($"Unexpected reply: {redisResult.Flag} {msg}");
			return redisResult;
		}

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



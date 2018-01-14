using Cassandra;
using System;
using System.Collections.Generic;
using System.Net;

namespace DotnetSpider.Extension.Infrastructure
{
	/// <summary>
	/// EndPoints=localhost;User=root;Port=3306;Password=None;
	/// Password should not contains ';'.
	/// </summary>
	public class CassandraConnectionSetting
	{
		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="connectString">数据库连接字符串</param>
		public CassandraConnectionSetting(string connectString)
		{
			Dictionary<string, string> settings = new Dictionary<string, string>();
			var parts = connectString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var part in parts)
			{
				var keyPair = part.Split('=');
				settings.Add(keyPair[0].Trim(), keyPair[1].Trim());
			}
			if (settings.ContainsKey("User"))
			{
				User = settings["User"];
			}
			if (settings.ContainsKey("Password"))
			{
				Password = settings["Password"];
			}
			if (settings.ContainsKey("Port"))
			{
				Port = int.Parse(settings["Port"]);
			}
			else
			{
				Port = 9042;
			}
			if (settings.ContainsKey("Host"))
			{
				Host = settings["Host"];
			}
		}

		/// <summary>
		/// 数据库地址
		/// </summary>
		public string Host { get; set; }

		/// <summary>
		/// 数据库的 endpoint
		/// </summary>
		public List<IPEndPoint> EndPoints
		{
			get
			{
				var ips = Host.Split(',');
				List<IPEndPoint> endPoints = new List<IPEndPoint>();
				foreach (var ip in ips)
				{
					endPoints.Add(new IPEndPoint(IPAddress.Parse(ip.Trim()), Port));
				}
				return endPoints;
			}
		}

		/// <summary>
		/// 数据库用户名
		/// </summary>
		public string User { get; set; }

		/// <summary>
		/// 数据库密码
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// 数据库端口
		/// </summary>
		public int Port { get; set; }

		/// <summary>
		/// 是否使用帐号密码登录数据库
		/// </summary>
		public bool WithCredentials
		{
			get
			{
				return !string.IsNullOrEmpty(User);
			}
		}
	}

	/// <summary>
	/// Cassandra数据库的帮助类
	/// </summary>
	public class CassandraUtil
	{
		/// <summary>
		/// 取得Cassandra的Cluster实现
		/// </summary>
		/// <param name="connectString">数据库连接字符串</param>
		/// <returns>Cluster实现</returns>
		public static Cluster CreateCluster(string connectString)
		{
			var connectSetting = new CassandraConnectionSetting(connectString);
			return CreateCluster(connectSetting);
		}

		/// <summary>
		/// 取得Cassandra的Cluster实现
		/// </summary>
		/// <param name="connectionSetting">数据库设置</param>
		/// <returns>Cluster实现</returns>
		public static Cluster CreateCluster(CassandraConnectionSetting connectionSetting)
		{
			var builder = Cluster.Builder()
				.AddContactPoints(connectionSetting.EndPoints);
			if (connectionSetting.WithCredentials)
			{
				builder.WithCredentials(connectionSetting.User, connectionSetting.Password);
			}
			var cluster = builder.Build();
			return cluster;
		}
	}
}

using Cassandra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Extension.Infrastructure
{
	/// <summary>
	/// EndPoints=localhost;User=root;Port=3306;Password=None;
	/// Password should not contains ';'.
	/// </summary>
	public class CassandraConnectionSetting
	{
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

		public string Host { get; set; }

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

		public string User { get; set; }

		public string Password { get; set; }

		public int Port { get; set; }

		public bool WithCredentials
		{
			get
			{
				return !string.IsNullOrEmpty(User);
			}
		}
	}


	public class CassandraUtils
	{
		public static Cluster CreateCluster(string connectString)
		{
			var connectSetting = new CassandraConnectionSetting(connectString);
			return CreateCluster(connectSetting);
		}

		public static Cluster CreateCluster(CassandraConnectionSetting connectSetting)
		{
			var builder = Cluster.Builder()
				.AddContactPoints(connectSetting.EndPoints);
			if (connectSetting.WithCredentials)
			{
				builder.WithCredentials(connectSetting.User, connectSetting.Password);
			}
			var cluster = builder.Build();
			return cluster;
		}
	}
}

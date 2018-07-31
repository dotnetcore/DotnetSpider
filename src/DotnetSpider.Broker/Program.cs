using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DotnetSpider.Broker
{
	class Program
	{
		static void Main(string[] args)
		{
			var config = new Dictionary<string, object>
			{
				{ "bootstrap.servers", "192.168.90.106:9092" },
				{"socket.blocking.max.ms", 1}
			};

			using (var producer = new Producer<Null, string>(config, null, new StringSerializer(Encoding.UTF8)))
			{
				for (int i = 0; i < 1000; ++i)
				{
					var dr = producer.ProduceAsync("my-topic", null, "test message text").Result;
					Console.WriteLine($"Delivered '{dr.Value}' to: {dr.TopicPartitionOffset}");
				}
			}
			Console.Read();
		}
	}
}

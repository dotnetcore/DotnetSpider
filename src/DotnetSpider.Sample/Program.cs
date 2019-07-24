using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using DotnetSpider.Common;
using DotnetSpider.EventBus;
using DotnetSpider.Kafka;
using DotnetSpider.Sample.samples;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;


namespace DotnetSpider.Sample
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var configure = new LoggerConfiguration()
#if DEBUG
				.MinimumLevel.Verbose()
#else
				.MinimumLevel.Information()
#endif
				.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
				.Enrich.FromLogContext()
				.WriteTo.Console().WriteTo
				.RollingFile("dotnet-spider.log");
			Log.Logger = configure.CreateLogger();

			var productConfig = new ProducerConfig
			{
				BootstrapServers = "zousong.com:9092",
				Partitioner = Partitioner.ConsistentRandom,
				SaslUsername = "user",
				SaslPassword = "bitnami",
				SaslMechanism = SaslMechanism.Plain,
				SecurityProtocol = SecurityProtocol.SaslPlaintext
			};
			var builder =
				new ProducerBuilder<Null, Event>(productConfig).SetValueSerializer(new ProtobufSerializer<Event>());

			var producer = builder.Build();
			for (int i = 0; i < 50; ++i)
			{
				producer.Produce("test", new Message<Null, Event> {Value = new Event {Data = "hi"}});
			}

			var config = new ConsumerConfig
			{
				BootstrapServers = "zousong.com:9092",
				GroupId = "agent",
				SaslUsername = "user",
				SaslPassword = "bitnami",
				SaslMechanism = SaslMechanism.Plain,
				SecurityProtocol = SecurityProtocol.SaslPlaintext,
				// Note: The AutoOffsetReset property determines the start offset in the event
				// there are not yet any committed offsets for the consumer group for the
				// topic/partitions of interest. By default, offsets are committed
				// automatically, so in this example, consumption will only start from the
				// earliest message in the topic 'my-topic' the first time you run the program.
				AutoOffsetReset = AutoOffsetReset.Earliest
			};
			using (var c = new ConsumerBuilder<Null, Event>(config)
				.SetValueDeserializer(new ProtobufDeserializer<Event>()).Build())
			{
				c.Subscribe("test");

				while (true)
				{
					Event msg = null;
					try
					{
						msg = c.Consume().Value;
						Console.WriteLine(msg);
					}

					catch (Exception e)
					{
						Console.WriteLine(e);
					}
				}
			}


			Console.Read();

			Startup.Execute<EntitySpider>(args);

			// await DistributedSpider.Run(); 
			Console.Read();
		}
	}
}
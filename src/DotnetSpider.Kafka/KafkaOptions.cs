using System;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

namespace DotnetSpider.Kafka
{
	public class KafkaOptions
	{
		private readonly IConfiguration _configuration;

		public KafkaOptions(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public string[] PartitionTopics => _configuration.GetSection("PartitionTopics").Get<string[]>();

		/// <summary>
		/// Kafka 服务地址
		/// </summary>
		public string KafkaBootstrapServers => string.IsNullOrWhiteSpace(_configuration["KafkaBootstrapServers"])
			? "localhost:9092"
			: _configuration["KafkaBootstrapServers"];

		/// <summary>
		/// Kafka 消费组
		/// </summary>
		public string KafkaConsumerGroup => string.IsNullOrWhiteSpace(_configuration["KafkaConsumerGroup"])
			? "DotnetSpider"
			: _configuration["KafkaConsumerGroup"];

		public string KafkaSaslUsername => string.IsNullOrWhiteSpace(_configuration["KafkaSaslUsername"])
			? null
			: _configuration["KafkaSaslUsername"];

		public string KafkaSaslPassword => string.IsNullOrWhiteSpace(_configuration["KafkaSaslPassword"])
			? null
			: _configuration["KafkaSaslPassword"];

		public SaslMechanism? KafkaSaslMechanism => string.IsNullOrWhiteSpace(_configuration["KafkaSaslMechanism"])
			? default(SaslMechanism?)
			: (SaslMechanism) Enum.Parse(typeof(SaslMechanism), _configuration["KafkaSaslMechanism"]);


		public SecurityProtocol? KafkaSecurityProtocol =>
			string.IsNullOrWhiteSpace(_configuration["KafkaSecurityProtocol"])
				? default(SecurityProtocol?)
				: (SecurityProtocol) Enum.Parse(typeof(SecurityProtocol), _configuration["KafkaSecurityProtocol"]);


		public int KafkaTopicPartitionCount => string.IsNullOrWhiteSpace(_configuration["KafkaTopicPartitionCount"])
			? 50
			: int.Parse(_configuration["KafkaTopicPartitionCount"]);
	}
}
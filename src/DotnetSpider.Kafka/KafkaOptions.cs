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

		public string[] PartitionTopics => string.IsNullOrWhiteSpace(_configuration["KafkaPartitionTopics"])
			? new string[0]
			: _configuration["KafkaPartitionTopics"].Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);

		/// <summary>
		/// Kafka 服务地址
		/// </summary>
		public string BootstrapServers => string.IsNullOrWhiteSpace(_configuration["KafkaBootstrapServers"])
			? "localhost:9092"
			: _configuration["KafkaBootstrapServers"];

		/// <summary>
		/// Kafka 消费组
		/// </summary>
		public string ConsumerGroup => string.IsNullOrWhiteSpace(_configuration["KafkaConsumerGroup"])
			? "DotnetSpider"
			: _configuration["KafkaConsumerGroup"];

		public string SaslUsername => string.IsNullOrWhiteSpace(_configuration["KafkaSaslUsername"])
			? null
			: _configuration["KafkaSaslUsername"];

		public string SaslPassword => string.IsNullOrWhiteSpace(_configuration["KafkaSaslPassword"])
			? null
			: _configuration["KafkaSaslPassword"];

		public SaslMechanism? SaslMechanism => string.IsNullOrWhiteSpace(_configuration["KafkaSaslMechanism"])
			? default(SaslMechanism?)
			: (SaslMechanism) Enum.Parse(typeof(SaslMechanism), _configuration["KafkaSaslMechanism"]);


		public SecurityProtocol? SecurityProtocol =>
			string.IsNullOrWhiteSpace(_configuration["KafkaSecurityProtocol"])
				? default(SecurityProtocol?)
				: (SecurityProtocol) Enum.Parse(typeof(SecurityProtocol), _configuration["KafkaSecurityProtocol"]);


		public int TopicPartitionCount => string.IsNullOrWhiteSpace(_configuration["KafkaTopicPartitionCount"])
			? 50
			: int.Parse(_configuration["KafkaTopicPartitionCount"]);
	}
}
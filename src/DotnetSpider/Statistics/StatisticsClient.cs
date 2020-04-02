using System.Threading.Tasks;
using DotnetSpider.Extensions;
using DotnetSpider.Statistics.Message;
using SwiftMQ;

namespace DotnetSpider.Statistics
{
	public class StatisticsClient : IStatisticsClient
	{
		private readonly IMessageQueue _messageQueue;

		public StatisticsClient(IMessageQueue messageQueue)
		{
			_messageQueue = messageQueue;
		}

		public async Task IncreaseTotalAsync(string id, long count)
		{
			await _messageQueue.PublishAsBytesAsync(TopicNames.Statistics, new Total(id, count));
		}

		public async Task IncreaseSuccessAsync(string id)
		{
			await _messageQueue.PublishAsBytesAsync(TopicNames.Statistics, new Success(id));
		}

		public async Task IncreaseFailureAsync(string id)
		{
			await _messageQueue.PublishAsBytesAsync(TopicNames.Statistics, new Failure(id));
		}

		public async Task StartAsync(string id, string name)
		{
			await _messageQueue.PublishAsBytesAsync(TopicNames.Statistics, new Start(id, name));
		}

		public async Task ExitAsync(string id)
		{
			await _messageQueue.PublishAsBytesAsync(TopicNames.Statistics, new Exit(id));
		}

		public async Task RegisterAgentAsync(string agentId, string agentName)
		{
			await _messageQueue.PublishAsBytesAsync(TopicNames.Statistics, new RegisterAgent(agentId, agentName));
		}

		public async Task IncreaseAgentSuccessAsync(string agentId, int elapsedMilliseconds)
		{
			await _messageQueue.PublishAsBytesAsync(TopicNames.Statistics,
				new AgentSuccess(agentId, elapsedMilliseconds));
		}

		public async Task IncreaseAgentFailureAsync(string agentId, int elapsedMilliseconds)
		{
			await _messageQueue.PublishAsBytesAsync(TopicNames.Statistics,
				new AgentFailure(agentId, elapsedMilliseconds));
		}

		public async Task PrintAsync(string id)
		{
			await _messageQueue.PublishAsBytesAsync(TopicNames.Statistics,
				new Print(id));
		}
	}
}

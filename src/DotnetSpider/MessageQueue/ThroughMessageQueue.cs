using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DotnetSpider.Common;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.MessageQueue
{
	/// <summary>
	/// 1. 发布会把消息推送到所有订阅了对应 topic 的消费者
	/// 2. 只能对 topic 做取消订阅，会导致所有订阅都取消。
	/// </summary>
	public class ThroughMessageQueue : IMq
	{
		private readonly ConcurrentDictionary<string, dynamic> _consumers =
			new ConcurrentDictionary<string, dynamic>();

		private readonly ILogger _logger;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="logger">日志接口</param>
		public ThroughMessageQueue(ILogger<ThroughMessageQueue> logger)
		{
			_logger = logger;
		}

		/// <summary>
		/// 推送消息到指定 topic
		/// </summary>
		/// <param name="topic"></param>
		/// <param name="message">消息</param>
		/// <returns></returns>
		public Task PublishAsync<TData>(string topic, MessageData<TData> message)
		{
			return Task.Factory.StartNew(() => { Publish(topic, message); });
		}

		private void Publish<TData>(string topic, MessageData<TData> message)
		{
			if (message == null)
			{
#if DEBUG
				var stackTrace = new System.Diagnostics.StackTrace();
				_logger.LogDebug($"Publish empty event: {stackTrace}");
#endif
				return;
			}

			message.Timestamp = (long) DateTimeHelper.GetCurrentUnixTimeNumber();
			if (_consumers.TryGetValue(topic, out var consumer))
			{
				try
				{
					consumer(message);
				}
				catch (Exception e)
				{
					_logger.LogError($"Consume message {message} on event {topic} failed: {e}");
				}
			}
			else
			{
#if DEBUG
				var stackTrace = new System.Diagnostics.StackTrace();
				_logger.LogDebug($"Event {topic} is not subscribed: {stackTrace}");
#endif
			}
		}

		/// <summary>
		/// 订阅事件
		/// </summary>
		/// <param name="topic"></param>
		/// <param name="action">消息消费的方法</param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Subscribe<TData>(string topic, Action<MessageData<TData>> action)
		{
			_consumers.AddOrUpdate(topic, x => action, (t, a) => action);
			_logger.LogInformation($"Subscribe: {topic}");
		}

		/// <summary>
		/// 取消订阅 topic
		/// </summary>
		/// <param name="topic">topic</param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Unsubscribe(string topic)
		{
			_consumers.TryRemove(topic, out _);
		}

		public void Dispose()
		{
		}
	}
}

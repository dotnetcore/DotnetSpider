using System;
using System.Threading.Tasks;

namespace DotnetSpider.EventBus
{
	/// <summary>
	/// 消息队列接口
	/// 因为业务上所有定阅都不需要负载，因此不存在多个客户端订阅同一个 topic 的情况，不需要 Unsubscribe 的实现
	/// </summary>
	public interface IEventBus : IDisposable
	{
		/// <summary>
		/// 推送消息到指定 topic
		/// </summary>
		/// <param name="topic"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		Task PublishAsync(string topic, Event message);

		/// <summary>
		/// 订阅 topic
		/// </summary>
		/// <param name="topic"></param>
		/// <param name="action"></param>
		void Subscribe(string topic, Action<Event> action);

		/// <summary>
		/// 取消订阅 topic
		/// </summary>
		/// <param name="topic"></param>
		void Unsubscribe(string topic);
	}
}
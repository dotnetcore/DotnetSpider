using System;
using System.Threading.Tasks;

namespace DotnetSpider.Infrastructure
{
	public interface IMqAdapter
	{
		Task PublishAsync<TMessage>(string queue, TMessage message);
		Task ConsumeAsync<TMessage>(Consumer<TMessage> consumer);
	}
}

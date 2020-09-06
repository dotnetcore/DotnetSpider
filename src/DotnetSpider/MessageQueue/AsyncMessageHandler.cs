using System.Threading.Tasks;

namespace DotnetSpider.MessageQueue
{
	public delegate Task AsyncMessageHandler<in TMessage>(TMessage message);
}
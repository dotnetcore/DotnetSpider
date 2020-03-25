using System.Threading.Tasks;

namespace SwiftMQ
{
	public delegate Task AsyncEventHandler<in TEvent>( TEvent @event);
}

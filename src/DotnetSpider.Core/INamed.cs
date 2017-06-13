
namespace DotnetSpider.Core
{
	public interface INamed
	{
		string Name { get; set; }
	}

	public abstract class Named
	{
		public string Name => GetType().Name;
	}
}

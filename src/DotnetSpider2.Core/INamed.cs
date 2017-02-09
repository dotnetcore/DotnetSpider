
namespace DotnetSpider.Core
{
	public interface INamed
	{
		string Name { get; }
	}

	public abstract class Named
	{
		public string Name
		{
			get
			{
				return GetType().Name;
			}
		}
	}
}

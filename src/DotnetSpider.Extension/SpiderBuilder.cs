using DotnetSpider.Core;

namespace DotnetSpider.Extension
{
	public abstract class EntitySpiderBuilder : IRunable
	{
		protected abstract EntitySpider GetEntitySpider();


		public virtual void Run(params string[] args)
		{
			var spider = GetEntitySpider();
			spider?.Run(args);
		}
	}
}
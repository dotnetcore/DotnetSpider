using System.Threading.Tasks;
using DotnetSpider.Core;

namespace DotnetSpider.Extension
{
	public abstract class EntitySpiderBuilder : IRunable, INamed,IIdentity
	{
		protected EntitySpiderBuilder(string name)
		{
			Name = name;
		}

		public string Name { get; set; }
		public string Identity { get; set; }

		public void Run(params string[] arguments)
		{
			var spider = GetEntitySpider();
			if (spider != null)
			{
				spider.Identity = Identity;
				spider.Run();
			}
		}

		public Task RunAsync(params string[] arguments)
		{
			return Task.Factory.StartNew(() =>
			{
				Run(arguments);
			});
		}

		protected abstract EntitySpider GetEntitySpider();
	}
}
using System;
using System.Threading.Tasks;
using DotnetSpider.Core;

namespace DotnetSpider.Extension
{
	public abstract class EntitySpiderBuilder : IRunable
	{
		protected abstract EntitySpider GetEntitySpider();
		protected EntitySpider Spider { get; private set; }

		public EntitySpiderBuilder()
		{
			Spider = GetEntitySpider();
			if (Spider == null)
			{
				throw new SpiderException("Spider is null.");
			}
		}

		public virtual void Run(params string[] args)
		{
			Spider.Run(args);
		}

		public Task RunAsync(params string[] arguments)
		{
			return Task.Factory.StartNew(() =>
			{
				Run(arguments);
			});
		}

		public void Pause(Action action = null)
		{
			Spider.Pause(action);
		}

		public void Exit(Action action = null)
		{
			Spider.Exit(action);
		}

		public void Contiune()
		{
			Spider.Contiune();
		}
	}
}
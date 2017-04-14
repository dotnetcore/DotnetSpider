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
		}

		public virtual void Run(params string[] args)
		{
			Spider?.Run(args);
		}

		public Task RunAsync(params string[] arguments)
		{
			return Task.Factory.StartNew(() =>
			{
				Run(arguments);
			});
		}

		public void Pause()
		{
			Spider?.Pause();
		}

		public void Exit()
		{
			Spider?.Exit();
		}

		public void Contiune()
		{
			Spider?.Contiune();
		}
	}
}
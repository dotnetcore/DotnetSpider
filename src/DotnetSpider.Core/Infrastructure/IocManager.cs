using Autofac;
using System;

namespace DotnetSpider.Core.Infrastructure
{
	public static class IocManager
	{
		private static IContainer _container;

		private static readonly Lazy<ContainerBuilder> Builder = new Lazy<ContainerBuilder>(() => new ContainerBuilder());

		static IocManager()
		{
			_container = Builder.Value.Build();
		}

		public static void Register<TIt, T>()
		{
			Builder.Value.RegisterType<T>().As<TIt>();
			_container = Builder.Value.Build();
		}

		public static TIt Resolve<TIt>()
		{
			TIt o;
			_container.TryResolve(out o);
			return o;
		}

	}
}

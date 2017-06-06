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

		public static void Register<IT, T>()
		{
			Builder.Value.RegisterType<T>().As<IT>();
			_container = Builder.Value.Build();
		}

		public static IT Resolve<IT>()
		{
			IT o;
			_container.TryResolve<IT>(out o);
			return o;
		}

	}
}

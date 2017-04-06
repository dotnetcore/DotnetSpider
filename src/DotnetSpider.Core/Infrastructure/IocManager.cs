using Autofac;
using System;
using System.Collections.Generic;

namespace DotnetSpider.Core.Infrastructure
{
	public static class IocManager
	{
		private static IContainer container;

		private static readonly Lazy<ContainerBuilder> builder = new Lazy<ContainerBuilder>(() =>
		{
			return new ContainerBuilder();
		});

		static IocManager()
		{
			container = builder.Value.Build();
		}

		public static void Register<IT, T>()
		{
			builder.Value.RegisterType<T>().As<IT>();
			container = builder.Value.Build();
		}

		public static IT Resolve<IT>()
		{
			IT o;
			container.TryResolve<IT>(out o);
			return o;
		}

	}
}

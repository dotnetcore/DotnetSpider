using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace DotnetSpider.Core
{
	public class IocContainer
	{
		private readonly IServiceCollection _services;
		private IServiceProvider _serviceProvider;

		private static readonly IocContainer Container = new IocContainer();

		public IocContainer()
		{
			_services = new ServiceCollection();
			_serviceProvider = _services.BuildServiceProvider();
		}

		public static IocContainer Default
		{
			get
			{
				lock (Container)
				{
					return Container;
				}
			}
		}

		public void AddTransient(Type serviceType, Type implementationType)
		{
			_serviceProvider = _services.AddTransient(serviceType, implementationType).BuildServiceProvider();
		}

		public void AddTransient(Type serviceType, Func<IServiceProvider, object> implementationFactory)
		{
			_serviceProvider = _services.AddTransient(serviceType, implementationFactory).BuildServiceProvider();
		}

		public void AddTransient<TService, TImplementation>()
			where TService : class where TImplementation : class, TService
		{
			_serviceProvider = _services.AddTransient<TService, TImplementation>().BuildServiceProvider();
		}

		public void AddTransient(Type serviceType)
		{
			_serviceProvider = _services.AddTransient(serviceType).BuildServiceProvider();
		}

		public void AddTransient<TService>() where TService : class
		{
			_serviceProvider = _services.AddTransient<TService>().BuildServiceProvider();
		}

		public void AddTransient<TService>(Func<IServiceProvider, TService> implementationFactory)
			where TService : class
		{
			_serviceProvider = _services.AddTransient(implementationFactory).BuildServiceProvider();
		}

		public void AddTransient<TService, TImplementation>(
			Func<IServiceProvider, TImplementation> implementationFactory) where TService : class
			where TImplementation : class, TService
		{
			_serviceProvider = _services.AddTransient<TService, TImplementation>(implementationFactory).BuildServiceProvider();
		}

		public void AddScoped(Type serviceType, Type implementationType)
		{
			_serviceProvider = _services.AddScoped(serviceType, implementationType).BuildServiceProvider();
		}

		public void AddScoped(Type serviceType, Func<IServiceProvider, object> implementationFactory)
		{
			_serviceProvider = _services.AddScoped(serviceType, implementationFactory).BuildServiceProvider();
		}

		public void AddScoped<TService, TImplementation>() where TService : class
			where TImplementation : class, TService
		{
			_serviceProvider = _services.AddScoped<TService, TImplementation>().BuildServiceProvider();
		}

		public void AddScoped(Type serviceType)
		{
			_serviceProvider = _services.AddScoped(serviceType).BuildServiceProvider();
		}

		public void AddScoped<TService>() where TService : class
		{
			_serviceProvider = _services.AddScoped<TService>().BuildServiceProvider();
		}

		public void AddScoped<TService>(Func<IServiceProvider, TService> implementationFactory) where TService : class
		{
			_serviceProvider = _services.AddScoped(implementationFactory).BuildServiceProvider();
		}

		public void AddScoped<TService, TImplementation>(Func<IServiceProvider, TImplementation> implementationFactory) where TService : class where TImplementation : class, TService
		{
			_serviceProvider = _services.AddScoped<TService, TImplementation>(implementationFactory).BuildServiceProvider();
		}

		public void AddSingleton(Type serviceType, Type implementationType)
		{
			_serviceProvider = _services.AddSingleton(serviceType, implementationType).BuildServiceProvider();
		}

		public void AddSingleton(Type serviceType, Func<IServiceProvider, object> implementationFactory)
		{
			_serviceProvider = _services.AddSingleton(serviceType, implementationFactory).BuildServiceProvider();
		}

		public void AddSingleton<TService, TImplementation>() where TService : class where TImplementation : class, TService
		{
			_serviceProvider = _services.AddSingleton<TService, TImplementation>().BuildServiceProvider();
		}

		public void AddSingleton(Type serviceType)
		{
			_serviceProvider = _services.AddSingleton(serviceType).BuildServiceProvider();
		}

		public void AddSingleton<TService>() where TService : class
		{
			_serviceProvider = _services.AddSingleton<TService>().BuildServiceProvider();
		}

		public void AddSingleton<TService>(Func<IServiceProvider, TService> implementationFactory) where TService : class
		{
			_serviceProvider = _services.AddSingleton(implementationFactory).BuildServiceProvider();
		}

		public void AddSingleton<TService, TImplementation>(Func<IServiceProvider, TImplementation> implementationFactory) where TService : class where TImplementation : class, TService
		{
			_serviceProvider = _services.AddSingleton<TService, TImplementation>(implementationFactory).BuildServiceProvider();
		}

		public void AddSingleton(Type serviceType, object implementationInstance)
		{
			_serviceProvider = _services.AddSingleton(serviceType, implementationInstance).BuildServiceProvider();
		}

		public void AddSingleton<TService>(TService implementationInstance) where TService : class
		{
			_serviceProvider = _services.AddSingleton(implementationInstance).BuildServiceProvider();
		}

		public T GetService<T>()
		{
			return _serviceProvider.GetService<T>();
		}

		public object GetRequiredService(Type serviceType)
		{
			return _serviceProvider.GetService(serviceType);
		}

		public T GetRequiredService<T>()
		{
			return _serviceProvider.GetRequiredService<T>();
		}

		public IEnumerable<T> GetServices<T>()
		{
			return _serviceProvider.GetServices<T>();
		}

		public IEnumerable<object> GetServices(Type serviceType)
		{
			return _serviceProvider.GetServices(serviceType);
		}
	}
}

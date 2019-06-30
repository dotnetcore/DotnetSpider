using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DotnetSpider.Core;
using DotnetSpider.Statistics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotnetSpider
{
	public class SpiderHostBuilder
	{
		private readonly ServiceCollection _services;
		private IConfiguration _configuration;
		private bool _hostBuilt;

		private readonly List<Action<IConfigurationBuilder>> _configureConfigActions =
			new List<Action<IConfigurationBuilder>>();

		private readonly List<Action<IServiceCollection>> _configureServiceActions =
			new List<Action<IServiceCollection>>();

		private List<IHostedService> _backgroundServices = new List<IHostedService>();

		private IServiceProvider _serviceProvider;

		private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		public SpiderHostBuilder()
		{
			_services=new ServiceCollection();
		}
		
		public SpiderHostBuilder ConfigureAppConfiguration(
			Action<IConfigurationBuilder> configure)
		{
			if (configure != null)
			{
				_configureConfigActions.Add(configure);
			}

			return this;
		}

		public SpiderHostBuilder ConfigureLogging(Action<ILoggingBuilder> configureLogging)
		{
			_services.AddLogging(configureLogging);
			return this;
		}

		public SpiderHostBuilder ConfigureServices(Action<IServiceCollection> configure)
		{
			if (configure != null)
			{
				_configureServiceActions.Add(configure);
			}

			return this;
		}

		public SpiderHostBuilder Register<TSpider>() where TSpider : Spider
		{
			_services.AddTransient<TSpider>();
			return this;
		}

		public SpiderHostBuilder Register(Type type)
		{
			if (!typeof(Spider).IsAssignableFrom(type))
			{
				throw new SpiderException("不能添加非爬虫类型");
			}

			_services.AddTransient(type);
			return this;
		}

		public SpiderProvider Build()
		{
			if (_hostBuilt)
			{
				throw new InvalidOperationException("Build can only be called once.");
			}

			_hostBuilt = true;

			BuildConfiguration();
			
			_services.AddScoped<ISpiderOptions, SpiderOptions>();
			
			CreateServiceProvider();

			_backgroundServices = _serviceProvider.GetServices<IHostedService>().ToList();

			foreach (var hostedService in _backgroundServices)
			{
				hostedService.StartAsync(_cancellationTokenSource.Token);
			}

			Console.CancelKeyPress += (sender, arguments) => { _cancellationTokenSource.Cancel(); };

			return new SpiderProvider(_serviceProvider);
		}

		private void CreateServiceProvider( )
		{
			_services.AddSingleton(_configuration);
			_services.AddSingleton<IStatisticsService, StatisticsService>();
			_services.AddScoped<ISpiderOptions, SpiderOptions>();
			_services.AddScoped<Spider>();
			_services.AddOptions();
			_services.AddLogging();

			foreach (Action<IServiceCollection> configureServicesAction in _configureServiceActions)
			{
				configureServicesAction(_services);
			}

			_serviceProvider = _services.BuildServiceProvider();
		}

		private void BuildConfiguration()
		{
			ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();

			foreach (Action<IConfigurationBuilder> configureAppConfigAction in
				_configureConfigActions)
			{
				configureAppConfigAction(configurationBuilder);
			}

			_configuration = configurationBuilder.Build();
		}
	}
}
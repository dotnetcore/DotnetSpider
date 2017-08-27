using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;

namespace DotnetSpider.Core.Infrastructure.Database
{
	public abstract class DbProviderFactories
	{
		private static readonly ConcurrentDictionary<string, DbProviderFactory> Configs = new ConcurrentDictionary<string, DbProviderFactory>();

		static DbProviderFactories()
		{
			RegisterFactory("System.Data.SqlClient", SqlClientFactory.Instance);
		}

		public static DbProviderFactory GetFactory(string providerInvariantName)
		{
			if (Configs.ContainsKey(providerInvariantName))
			{
				DbProviderFactory factory;
				Configs.TryGetValue(providerInvariantName, out factory);
				if (factory == null)
				{
					throw new SpiderException("Provider not found.");
				}
				return factory;
			}
			throw new SpiderException("Provider not found.");
		}

		public static void RegisterFactory(string providerInvariantName, DbProviderFactory factory)
		{
			Configs.TryAdd(providerInvariantName, factory);
		}

		public static IEnumerable<string> GetFactoryProviderNames()
		{
			return Configs.Keys;
		}
	}
}

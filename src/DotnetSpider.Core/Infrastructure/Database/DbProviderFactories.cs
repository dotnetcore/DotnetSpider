using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System;
using System.Linq;
using System.Reflection;

namespace DotnetSpider.Core.Infrastructure.Database
{
	public abstract class DbProviderFactories
	{
		private static readonly ConcurrentDictionary<string, DbProviderFactory> Configs = new ConcurrentDictionary<string, DbProviderFactory>();
		private static readonly string[] dataProviders = new string[] { "mysql.data.dll", "npgsql.dll" };

		static DbProviderFactories()
		{
			RegisterFactory("System.Data.SqlClient", SqlClientFactory.Instance);

			var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

			foreach (var assembly in allAssemblies)
			{
				var dependenceProviderNames = assembly.GetReferencedAssemblies().Where(a => dataProviders.Contains($"{a.Name.ToLower()}.dll"));
				foreach (var assemblyName in dependenceProviderNames)
				{
					Assembly.Load(assemblyName);
				}
			}

			var baseProviders = new HashSet<Assembly>(AppDomain.CurrentDomain.GetAssemblies().Where(a => (dataProviders.Contains($"{a.GetName().Name.ToLower()}.dll"))));
			foreach (var assembly in baseProviders)
			{
				var factoryType = assembly.GetExportedTypes().FirstOrDefault(t => t.BaseType == typeof(DbProviderFactory));
				FieldInfo instanceField = factoryType.GetField("Instance");
				DbProviderFactory instance = instanceField?.GetValue(null) as DbProviderFactory;
				if (instance != null)
				{
					RegisterFactory(factoryType.Namespace, instance);
				}
			}
			var providerDlls = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory).Where(p => dataProviders.Contains(Path.GetFileName(p).ToLower())).ToList();
			foreach (var providerDll in providerDlls)
			{
				var assembly = Assembly.LoadFrom(providerDll);
				var factoryType = assembly.GetExportedTypes().FirstOrDefault(t => t.BaseType == typeof(DbProviderFactory));
				FieldInfo instanceField = factoryType.GetField("Instance");
				DbProviderFactory instance = instanceField?.GetValue(null) as DbProviderFactory;
				if (instance != null)
				{
					RegisterFactory(factoryType.Namespace, instance);
				}
			}
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

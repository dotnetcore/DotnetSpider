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
	/// <summary>
	/// Database Driver Factory.
	/// </summary>
	/// <summary xml:lang="zh-CN">
	/// 数据库驱动工厂
	/// </summary>
	public abstract class DbProviderFactories
	{
		/// <summary>
		/// MySql provider name
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// MySql驱动类名
		/// </summary>
		public const string MySqlProvider = "MySql.Data.MySqlClient";

		/// <summary>
		/// SqlServer provider name
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// SqlServer驱动类名
		/// </summary>
		public const string SqlServerProvider = "System.Data.SqlClient";

		/// <summary>
		/// PostgreSql provider name
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// PostgreSql
		/// </summary>
		public const string PostgreSqlProvider = "Npgsql";

		private static readonly ConcurrentDictionary<string, DbProviderFactory> Configs =
			new ConcurrentDictionary<string, DbProviderFactory>();

		private static readonly string[] DataProviders = { "mysqlconnector.dll", "mysql.data.dll", "npgsql.dll" };

		static DbProviderFactories()
		{
			RegisterFactory(SqlServerProvider, SqlClientFactory.Instance);

			var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

			foreach (var assembly in allAssemblies)
			{
				var dependenceProviderNames = assembly.GetReferencedAssemblies()
					.Where(a => DataProviders.Contains($"{a.Name.ToLower()}.dll"));
				foreach (var assemblyName in dependenceProviderNames)
				{
					Assembly.Load(assemblyName);
				}
			}

			var baseProviders = new HashSet<Assembly>(AppDomain.CurrentDomain.GetAssemblies()
				.Where(a => (DataProviders.Contains($"{a.GetName().Name.ToLower()}.dll"))));
			foreach (var assembly in baseProviders)
			{
				var factoryType = assembly.GetExportedTypes()
					.FirstOrDefault(t => t.BaseType == typeof(DbProviderFactory));
				if (factoryType != null)
				{
					FieldInfo instanceField = factoryType.GetField("Instance");
					if (instanceField?.GetValue(null) is DbProviderFactory instance)
					{
						RegisterFactory(factoryType.Namespace, instance);
					}
				}
			}
			var providerDlls = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory)
				.Where(p => DataProviders.Contains(Path.GetFileName(p).ToLower())).ToList();
			foreach (var providerDll in providerDlls)
			{
				if (string.IsNullOrWhiteSpace(providerDll))
				{
					continue;
				}
				var assembly = Assembly.Load(Path.GetFileName(providerDll).Replace(".dll", ""));
				var factoryType = assembly.GetExportedTypes()
					.FirstOrDefault(t => t.BaseType == typeof(DbProviderFactory));
				if (factoryType != null)
				{
					FieldInfo instanceField = factoryType.GetField("Instance");
					if (instanceField?.GetValue(null) is DbProviderFactory instance)
					{
						RegisterFactory(factoryType.Namespace, instance);
					}
				}
			}
		}

		/// <summary>
		/// Get <see cref="DbProviderFactory"/> from database driver provider name.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 通过驱动类名取得工厂类
		/// </summary>
		/// <param name="providerInvariantName">驱动类名</param>
		/// <returns>Represents a set of methods for creating instances of a provider's implementation of the data source classes.</returns>
		public static DbProviderFactory GetFactory(string providerInvariantName)
		{
			if (Configs.ContainsKey(providerInvariantName))
			{
				Configs.TryGetValue(providerInvariantName, out var factory);
				if (factory == null)
				{
					throw new SpiderException("Provider not found.");
				}
				return factory;
			}
			throw new SpiderException("Provider not found.");
		}

		/// <summary>
		/// Register <see cref="DbProviderFactory"/> to DbProviderFactories Configs.
		/// </summary>
		/// <summary xml:lang="zh-CN">
		/// 注册驱动
		/// </summary>
		/// <param name="providerInvariantName">驱动类名</param>
		/// <param name="factory">Represents a set of methods for creating instances of a provider's implementation of the data source classes.</param>
		public static void RegisterFactory(string providerInvariantName, DbProviderFactory factory)
		{
			Configs.TryAdd(providerInvariantName, factory);
		}
	}
}
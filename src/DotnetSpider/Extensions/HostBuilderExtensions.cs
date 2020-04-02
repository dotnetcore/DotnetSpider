using System;
using System.Reflection;
using DotnetSpider.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace DotnetSpider.Extensions
{
	public static class HostBuilderExtensions
	{
		public static IConfiguration GetConfiguration(this IHostBuilder builder)
		{
			builder.NotNull(nameof(builder));
			var type = builder.GetType();
			var field = GetConfigurationFieldInfo(type);
			if (field != null)
			{
				return field.GetValue(builder) as IConfiguration;
			}

			return null;
		}

		private static FieldInfo GetConfigurationFieldInfo(Type type)
		{
			var fields = type.GetField("_appConfiguration",
				BindingFlags.NonPublic | BindingFlags.Instance);

			if (fields != null)
			{
				return fields;
			}
			else
			{
				return type.BaseType == null ? null : GetConfigurationFieldInfo(type.BaseType);
			}
		}
	}
}

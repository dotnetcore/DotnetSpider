using System;
using Microsoft.Extensions.Configuration;

namespace DotnetSpider.DataFlow.Storage
{
	internal static class StorageUtilities
	{
		internal static IDataFlow CreateStorage(string storageType, IConfiguration configuration)
		{
			if (string.IsNullOrWhiteSpace(storageType))
			{
				throw new ArgumentNullException($"Storage is not configured");
			}

			var type = Type.GetType(storageType);
			if (type == null)
			{
				throw new SpiderException($"Storage {storageType} not found");
			}

			// if (!typeof(StorageBase).IsAssignableFrom(type) && !typeof(EntityStorageBase).IsAssignableFrom(type))
			// {
			// 	throw new SpiderException($"{type} is not a storage dataFlow");
			// }

			var method = type.GetMethod("CreateFromOptions");

			if (method == null)
			{
				throw new SpiderException($"Storage {type} didn't implement method CreateFromOptions");
			}

			var storage = method.Invoke(null, new object[] {configuration});
			if (storage == null)
			{
				throw new SpiderException("Create default storage failed");
			}

			return (IDataFlow)storage;
		}
	}
}

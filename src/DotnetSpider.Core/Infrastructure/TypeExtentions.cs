using System;
using System.Reflection;

namespace DotnetSpider.Core.Infrastructure
{
	public static class TypeExtentions
	{
		public static Type GetTypeCrossPlatform(this Type entityType)
		{
			return entityType;
		}
		public static Type GetTypeCrossPlatform(this TypeInfo entityType)
		{
			return entityType.AsType();
		}
		public static TypeInfo GetTypeInfoCrossPlatform(this Type entityType)
		{
			return entityType.GetTypeInfo();
		}
		public static TypeInfo GetTypeInfoCrossPlatform(this TypeInfo entityType)
		{
			return entityType;
		}
	}
}

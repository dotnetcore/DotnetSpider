using System;
using System.Linq;
using System.Reflection;

namespace DotnetSpider.Core.Infrastructure
{
	public abstract class Singleton<T>
	{
		private static readonly Lazy<T> MyInstance = new Lazy<T>(() =>
		{
			var ctors = typeof(T).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			if (ctors.Length != 1)
			{
				throw new InvalidOperationException(String.Format("Type {0} must have exactly one constructor.", typeof(T)));
			}
			var ctor = ctors.SingleOrDefault(c => !c.GetParameters().Any() && c.IsPrivate);
			if (ctor == null)
			{
				throw new InvalidOperationException(String.Format("The constructor for {0} must be private and take no parameters.", typeof(T)));
			}
			return (T)ctor.Invoke(null);
		});

		public static T Instance => MyInstance.Value;
	}
}

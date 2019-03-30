using System;
using System.Linq;
using System.Reflection;

namespace DotnetSpider.Core
{
    /// <summary>
    /// 单独的泛型实型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Singleton<T>
    {
        private static readonly Lazy<T> MyInstance = new Lazy<T>(() =>
        {
            var ctors = typeof(T).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (ctors.Length != 1)
            {
                throw new InvalidOperationException($"Type {typeof(T)} must have exactly one constructor.");
            }
            var ctor = ctors.SingleOrDefault(c => !c.GetParameters().Any() && c.IsPrivate);
            if (ctor == null)
            {
                throw new InvalidOperationException($"The constructor for {typeof(T)} must be private and take no parameters.");
            }
            return (T)ctor.Invoke(null);
        });

        /// <summary>
        /// 单例对象
        /// </summary>
        public static T Instance => MyInstance.Value;
    }
}
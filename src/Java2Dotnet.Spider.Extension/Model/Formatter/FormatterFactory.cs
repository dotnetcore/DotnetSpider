using System;
using System.Collections.Generic;
using System.Linq;

namespace Java2Dotnet.Spider.Extension.Model.Formatter
{
	public class FormatterFactory
	{
		private static readonly Dictionary<string, Type> Formatters = new Dictionary<string, Type>();

		static FormatterFactory()
		{
			var formatterType = typeof(Formatter);
#if !NET_CORE
			var types = formatterType.Assembly.GetTypes().Where(t => t.FullName != formatterType.FullName).ToList();
#else
			var types = formatterType.GetTypeInfo().Assembly.GetTypes().Where(t => t.FullName != formatterType.FullName).ToList();
#endif
			foreach (var type in types)
			{
				if (formatterType.IsAssignableFrom(type))
				{
					Formatters.Add(type.Name, type);
				}
			}
		}

		public static Type GetFormatterType(string name)
		{
			return Formatters.ContainsKey(name) ? Formatters[name] : null;
		}
	}
}

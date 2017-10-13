using System;
using System.Collections;
using System.Collections.Generic;

namespace DotnetSpider.Extension.Model
{
	public class DataObject : Dictionary<string, object>
	{
		public object TryGetValue(string name)
		{
			return ContainsKey(name) ? this[name] : null;
		}
	}
}

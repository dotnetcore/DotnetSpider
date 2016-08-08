using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetSpider.Core;

namespace DotnetSpider.Test
{
	public static class Assert
	{
		public static void AreEqual(dynamic actual, dynamic expect)
		{
			if (actual != expect)
			{
				throw new Exception($"Actual is {actual} but expect value is {expect}");
			}
		}

		internal static void IsTrue(bool v)
		{
			if (!v)
			{
				throw new Exception($"Not true");
			}
		}

		public static void IsNotNull(object value)
		{
			if (null != value)
			{
				throw new Exception($"Not true");
			}
		}

		public static void IsFalse(bool value)
		{
			if (!value)
			{
				throw new Exception($"Not False");
			}
		}

		public static void IsNull(object result1)
		{
			if (result1 != null)
			{
				throw new Exception($"Not False");
			}
		}
	}
}

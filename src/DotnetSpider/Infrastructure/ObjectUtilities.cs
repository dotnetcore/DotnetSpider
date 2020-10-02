using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.Infrastructure
{
	public class ObjectUtilities
	{
		public static void DisposeSafely(params object[] objs)
		{
			foreach (var obj in objs)
			{
				try
				{
					(obj as IDisposable)?.Dispose();
				}
				catch (Exception)
				{
					// ignored
				}
			}
		}

		public static void DisposeSafely(ILogger logger, params object[] objs)
		{
			DisposeSafely(logger, objs.AsEnumerable());
		}

		public static void DisposeSafely(ILogger logger, IEnumerable<object> objs)
		{
			foreach (var obj in objs)
			{
				try
				{
					(obj as IDisposable)?.Dispose();
				}
				catch (Exception e)
				{
					logger.LogWarning($"Dispose {obj} failed: {e}");
				}
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.Infrastructure;

public static class ObjectUtilities
{
    public static void DisposeSafely(params object[] objs)
    {
        foreach (var obj in objs)
        {
            try
            {
                (obj as IDisposable)?.Dispose();
            }
            catch
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
                logger.LogWarning(e, "Dispose object failed");
            }
        }
    }
}

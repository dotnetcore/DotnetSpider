using DotnetSpider.Core;
using DotnetSpider.Core.Monitor;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IocExtension.ServiceCollection.AddSingleton<IMonitorService, NLogMonitor>();

            JdSkuSampleSpider spiderBuilder = new JdSkuSampleSpider();
            spiderBuilder.Run("rerun");
        }
    }
}

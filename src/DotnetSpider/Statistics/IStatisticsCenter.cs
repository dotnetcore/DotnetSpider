#if !NET451
using Microsoft.Extensions.Hosting;

#else
using DotnetSpider.Core;
#endif

namespace DotnetSpider.Statistics
{
    /// <summary>
    /// 统计服务中心
    /// </summary>
    public interface IStatisticsCenter : IHostedService
    {        
    }
}
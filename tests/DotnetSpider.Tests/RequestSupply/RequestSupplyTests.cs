using System.Threading.Tasks;
using Xunit;

namespace DotnetSpider.Tests.RequestSupply
{
    public class RequestSupplyTests : TestBase
    {
        /// <summary>
        /// 测试 RelationalDatabaseRequestSupply 是否能正常工作
        /// </summary>
        /// <returns></returns>
        [Fact(DisplayName = "RelationalDatabaseRequestSupply")]
        public Task RelationalDatabaseRequestSupply()
        {
            // TODO: 
#if NETFRAMEWORK
            return DotnetSpider.Core.Framework.CompletedTask;
#else
            return Task.CompletedTask;
#endif
        }
    }
}
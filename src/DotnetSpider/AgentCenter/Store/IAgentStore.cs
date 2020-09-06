using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotnetSpider.AgentCenter.Store
{
    public interface IAgentStore
    {
        /// <summary>
        /// 创建数据库和表
        /// </summary>
        /// <returns></returns>
        Task EnsureDatabaseAndTableCreatedAsync();

        /// <summary>
        /// 查询所有已经注册的下载器代理
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<AgentInfo>> GetAllListAsync();

        /// <summary>
        /// 添加下载器代理
        /// </summary>
        /// <param name="agent">下载器代理</param>
        /// <returns></returns>
        Task RegisterAsync(AgentInfo agent);

        /// <summary>
        /// 保存下载器代理的心跳
        /// </summary>
        /// <param name="heartbeat">下载器代理心跳</param>
        /// <returns></returns>
        Task HeartbeatAsync(AgentHeartbeat heartbeat); 
    }
}
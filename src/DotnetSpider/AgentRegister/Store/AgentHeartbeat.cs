using System;
using System.ComponentModel.DataAnnotations;
using DotnetSpider.Infrastructure;

namespace DotnetSpider.AgentRegister.Store
{
    public class AgentHeartbeat
    {
        /// <summary>
        /// 节点标识
        /// </summary>
        public uint Id { get; private set; }

        /// <summary>
        /// 标识
        /// </summary>
        [StringLength(36)]
        public string AgentId { get; private set; }

        /// <summary>
        /// 名称
        /// </summary>
        [StringLength(255)]
        public string AgentName { get; private set; }

        /// <summary>
        /// 空闲内存
        /// </summary>
        public uint FreeMemory { get; private set; }

        /// <summary>
        /// 已经分配的下载器数量
        /// </summary>
        public uint DownloaderCount { get; private set; }

        /// <summary>
        /// 上报时间
        /// </summary>
        public DateTimeOffset CreationTime { get; private set; }

        public AgentHeartbeat(string agentId, string agentName, uint freeMemory, uint downloaderCount)
        {
            agentId.NotNullOrWhiteSpace(nameof(agentId));

            AgentId = agentId;
            AgentName = agentName;
            FreeMemory = freeMemory;
            DownloaderCount = downloaderCount;
            CreationTime = DateTimeOffset.Now;
        }
    }
}
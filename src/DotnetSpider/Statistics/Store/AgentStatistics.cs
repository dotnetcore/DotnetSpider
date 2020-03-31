using System.ComponentModel.DataAnnotations;

namespace DotnetSpider.Statistics.Store
{
    public class AgentStatistics
    {
        /// <summary>
        /// 节点标识
        /// </summary>
        [StringLength(36)]
        public virtual string Id { get; private set; }

        /// <summary>
        /// 下载成功数
        /// </summary>
        public virtual ulong Success { get; private set; }

        /// <summary>
        /// 下载失败数
        /// </summary>
        public virtual ulong Failure { get; private set; }

        /// <summary>
        /// 下载总消耗时间
        /// </summary>
        public virtual ulong ElapsedMilliseconds { get; private set; }

        public AgentStatistics(string id)
        {
            Id = id;
        }

        public void IncreaseSuccess()
        {
            Success += 1;
        }

        public void IncreaseFailure()
        {
            Failure += 1;
        }

        public void IncreaseElapsedMilliseconds(int elapsedMilliseconds)
        {
            ElapsedMilliseconds += (uint) elapsedMilliseconds;
        }
    }
}
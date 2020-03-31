using System;
using System.ComponentModel.DataAnnotations;
using DotnetSpider.Infrastructure;

namespace DotnetSpider.Statistics.Store
{
    public class SpiderStatistics
    {
        /// <summary>
        /// 爬虫标识
        /// </summary>
        [StringLength(36)]
        public virtual string Id { get; private set; }

        /// <summary>
        /// 爬虫名称
        /// </summary>
        [StringLength(255)]
        public virtual string Name { get; private set; }

        /// <summary>
        /// 爬虫开始时间
        /// </summary>
        public virtual DateTimeOffset? Start { get; private set; }

        /// <summary>
        /// 爬虫退出时间 
        /// </summary>
        public virtual DateTimeOffset? Exit { get; private set; }

        /// <summary>
        /// 链接总数
        /// </summary>
        public virtual ulong Total { get; private set; }

        /// <summary>
        /// 已经完成
        /// </summary>
        public virtual ulong Success { get; private set; }

        /// <summary>
        /// 失败链接数
        /// </summary>
        public virtual ulong Failure { get; private set; }

        public SpiderStatistics(string id)
        {
            id.NotNullOrWhiteSpace(nameof(id));

            Id = id;
        }

        public void SetName(string name)
        {
            name.NotNullOrWhiteSpace(nameof(name));
            Name = name;
        }

        public void OnStarted()
        {
            Start = DateTimeOffset.Now;
        }

        public void OnExited()
        {
            Exit = DateTimeOffset.Now;
        }

        public void IncrementSuccess()
        {
            Success += 1;
        }

        public void IncrementFailure()
        {
            Failure += 1;
        }

        public void IncrementTotal(long count)
        {
            Total += (ulong) count;
        }
    }
}
using System;
using System.ComponentModel.DataAnnotations;
using DotnetSpider.Infrastructure;

namespace DotnetSpider.AgentRegister.Store
{
    public class AgentInfo
    {
        /// <summary>
        /// 标识
        /// </summary>
        [StringLength(36)]
        public virtual string Id { get; private set; }

        /// <summary>
        /// 名称
        /// </summary>
        [StringLength(255)]
        public string Name { get; private set; }

        /// <summary>
        /// CPU 核心数
        /// </summary>
        public int ProcessorCount { get; private set; }

        /// <summary>
        /// 总内存
        /// </summary>
        public int TotalMemory { get; private set; }

        /// <summary>
        /// 上一次更新时间
        /// </summary>
        public DateTimeOffset LastModificationTime { get; private set; }

        /// <summary>
        /// 是否已经标记删除
        /// </summary>
        public bool IsDeleted { get; private set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Required]
        public DateTimeOffset CreationTime { get; private set; }

        /// <summary>
        /// 刷新上一次更新时间
        /// </summary>
        public void Refresh()
        {
            LastModificationTime = DateTimeOffset.Now;
        }

        public AgentInfo(string id, string name, int processorCount, int totalMemory)
        {
            id.NotNullOrWhiteSpace(nameof(id));
            name.NotNullOrWhiteSpace(nameof(name));

            Id = id;
            Name = name;
            ProcessorCount = processorCount;
            TotalMemory = totalMemory;
            CreationTime = DateTimeOffset.Now;
            LastModificationTime = CreationTime;
        }

        public bool Online => (DateTimeOffset.Now - LastModificationTime).TotalSeconds <= 30;
    }
}
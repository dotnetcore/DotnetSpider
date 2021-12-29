using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DotnetSpider.Infrastructure;

namespace DotnetSpider.AgentCenter.Store
{
	[Table("agent")]
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
		[Column("processor_count")]
		public int ProcessorCount { get; private set; }

		/// <summary>
		/// 总内存
		/// </summary>
		[Column("total_memory")]
		public long TotalMemory { get; private set; }

		/// <summary>
		/// 上一次更新时间
		/// </summary>
		[Column("last_modification_time")]
		public DateTimeOffset LastModificationTime { get; private set; }

		/// <summary>
		/// 是否已经标记删除
		/// </summary>
		[Column("deleted")]
		public bool Deleted { get; private set; }

		/// <summary>
		/// 创建时间
		/// </summary>
		[Required]
		[Column("creation_time")]
		public DateTimeOffset CreationTime { get; private set; }

		public AgentInfo(string id, string name, int processorCount, long totalMemory)
		{
			id.NotNullOrWhiteSpace(nameof(id));
			name.NotNullOrWhiteSpace(nameof(name));

			Id = id;
			Name = name;
			Deleted = false;
			ProcessorCount = processorCount;
			TotalMemory = totalMemory;
			CreationTime = DateTimeOffset.Now;
			LastModificationTime = CreationTime;
		}

		public bool Online => (DateTimeOffset.Now - LastModificationTime).TotalSeconds <= 30;

		/// <summary>
		/// 刷新上一次更新时间
		/// </summary>
		public void Refresh()
		{
			LastModificationTime = DateTimeOffset.Now;
		}

		public override string ToString()
		{
			return $"Id {Id}, CreationTime {CreationTime}, Deleted {Deleted}";
		}
	}
}

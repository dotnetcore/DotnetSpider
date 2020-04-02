using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotnetSpider.Statistics.Store
{
	[Table("agent_statistics")]
	public class AgentStatistics
	{
		/// <summary>
		/// 节点标识
		/// </summary>
		[StringLength(36)]
		[Column("agent_id")]
		public virtual string Id { get; private set; }

		/// <summary>
		/// 节点名称
		/// </summary>
		[Column("name")]
		public string Name { get; private set; }

		/// <summary>
		/// 下载成功数
		/// </summary>
		[Column("success")]
		public virtual long Success { get; private set; }

		/// <summary>
		/// 下载失败数
		/// </summary>
		[Column("failure")]
		public virtual long Failure { get; private set; }

		/// <summary>
		/// 下载总消耗时间
		/// </summary>
		[Column("elapsed_milliseconds")]
		public virtual long ElapsedMilliseconds { get; private set; }

		/// <summary>
		/// 上报时间
		/// </summary>
		[Column("creation_time")]
		public DateTimeOffset CreationTime { get; private set; }

		/// <summary>
		///
		/// </summary>
		[Column("last_modification_time")]
		public DateTimeOffset LastModificationTime { get; private set; }

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
			ElapsedMilliseconds += (uint)elapsedMilliseconds;
		}

		public void SetName(string name)
		{
			Name = name;
		}
	}
}

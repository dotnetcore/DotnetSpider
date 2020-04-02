using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DotnetSpider.Infrastructure;

namespace DotnetSpider.Statistics.Store
{
	[Table("statistics")]
	public class SpiderStatistics
	{
		/// <summary>
		/// 爬虫标识
		/// </summary>
		[StringLength(36)]
		[Column("id")]
		public virtual string Id { get; private set; }

		/// <summary>
		/// 爬虫名称
		/// </summary>
		[StringLength(255)]
		[Column("name")]
		public virtual string Name { get; private set; }

		/// <summary>
		/// 爬虫开始时间
		/// </summary>
		[Column("start")]
		public virtual DateTimeOffset? Start { get; private set; }

		/// <summary>
		/// 爬虫退出时间
		/// </summary>
		[Column("exit")]
		public virtual DateTimeOffset? Exit { get; private set; }

		/// <summary>
		/// 链接总数
		/// </summary>
		[Column("total")]
		public virtual long Total { get; private set; }

		/// <summary>
		/// 已经完成
		/// </summary>
		[Column("success")]
		public virtual long Success { get; private set; }

		/// <summary>
		/// 失败链接数
		/// </summary>
		[Column("failure")]
		public virtual long Failure { get; private set; }

		/// <summary>
		///
		/// </summary>
		[Column("last_modification_time")]
		public DateTimeOffset LastModificationTime { get; private set; }

		/// <summary>
		///
		/// </summary>
		[Column("creation_time")]
		public DateTimeOffset CreationTime { get; private set; }

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
			Total += count;
		}
	}
}

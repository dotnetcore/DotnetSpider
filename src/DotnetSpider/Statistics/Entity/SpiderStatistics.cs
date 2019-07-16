using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DotnetSpider.Common;

namespace DotnetSpider.Statistics.Entity
{
	/// <summary>
	/// 爬虫的统计信息
	/// </summary>
	[Table("spider_statistics")]
	public class SpiderStatistics
	{
		private readonly AtomicLong _total = new AtomicLong();
		private readonly AtomicLong _success = new AtomicLong();
		private readonly AtomicLong _failed = new AtomicLong();

		/// <summary>
		/// 主键
		/// </summary>
		[Key]
		[Column("owner_id")]
		public string OwnerId { get; set; }

		/// <summary>
		/// 爬虫启动时间
		/// </summary>
		[Column("start")]
		public DateTime? Start { get; set; }

		/// <summary>
		/// 爬虫结束时间
		/// </summary>
		[Column("exit")]
		public DateTime? Exit { get; set; }

		/// <summary>
		/// 总的请求数
		/// </summary>
		[Column("total")]
		public long Total
		{
			get => _total.Value;
			set => _total.Set(value);
		}

		/// <summary>
		/// 处理成功的请求数
		/// </summary>
		[Column("success")]
		public long Success
		{
			get => _success.Value;
			set => _success.Set(value);
		}

		/// <summary>
		/// 处理失败的请求数
		/// </summary>
		[Column("failed")]
		public long Failed
		{
			get => _failed.Value;
			set => _failed.Set(value);
		}

		/// <summary>
		/// 添加成功次数 1
		/// </summary>
		internal void IncSuccess()
		{
			_success.Inc();
		}

		/// <summary>
		/// 上一次更新时间
		/// </summary>
		[Column("last_modification_time")]
		public DateTime LastModificationTime { get; set; }
		
		/// <summary>
		/// 添加指定的失败次数
		/// </summary>
		/// <param name="count">失败次数</param>
		internal void AddFailed(int count = 1)
		{
			_failed.Add(count);
		}

		/// <summary>
		/// 添加请求总数
		/// </summary>
		/// <param name="count"></param>
		internal void AddTotal(int count)
		{
			_total.Add(count);
		}
	}
}
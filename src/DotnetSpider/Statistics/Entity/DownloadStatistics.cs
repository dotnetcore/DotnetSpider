using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DotnetSpider.Common;

namespace DotnetSpider.Statistics.Entity
{
	/// <summary>
	/// 下载代理器的统计信息
	/// </summary>
	[Table("download_statistics")]
	public class DownloadStatistics
	{
		private readonly AtomicLong _elapsedMilliseconds = new AtomicLong();
		private readonly AtomicLong _success = new AtomicLong();
		private readonly AtomicLong _failed = new AtomicLong();

		/// <summary>
		/// 下载代理器的标识
		/// </summary>
		[Column("agent_id")]
		[Key]
		public string AgentId { get; set; }

		/// <summary>
		/// 下载成功的次数
		/// </summary>
		[Column("success")]
		public long Success
		{
			get => _success.Value;
			set => _success.Set(value);
		}

		/// <summary>
		/// 下载失败的次数
		/// </summary>
		[Column("failed")]
		public long Failed
		{
			get => _failed.Value;
			set => _failed.Set(value);
		}

		/// <summary>
		/// 每次下载所需要的时间的总和
		/// </summary>
		[Column("elapsed_milliseconds")]
		public long ElapsedMilliseconds
		{
			get => _elapsedMilliseconds.Value;
			set => _elapsedMilliseconds.Set(value);
		}

		/// <summary>
		/// 添加下载所消耗的时间
		/// </summary>
		/// <param name="value"></param>
		internal void AddElapsedMilliseconds(long value)
		{
			_elapsedMilliseconds.Add(value);
		}

		/// <summary>
		/// 添加下载成功的次数
		/// </summary>
		/// <param name="count"></param>
		internal void AddSuccess(int count)
		{
			_success.Add(count);
		}

		/// <summary>
		/// 添加下载失败的次数
		/// </summary>
		/// <param name="count"></param>
		internal void AddFailed(int count)
		{
			_failed.Add(count);
		}
	}
}
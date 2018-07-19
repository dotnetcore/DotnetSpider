using System;

namespace DotnetSpider.Common
{
	/// <summary>
	/// 任务控制接口
	/// 实现任务的暂停、继续、退出
	/// </summary>
	public interface IControllable
	{
		ILogger Logger { get; }

		/// <summary>
		/// 暂停任务
		/// 暂停一个不在运行的任务应该提示警告
		/// </summary>
		/// <param name="action">暂停任务成功后回调的方法</param>
		void Pause(Action action = null);

		/// <summary>
		/// 继续任务
		/// 继续一个不在暂停的任务应该提示警告
		/// </summary>
		void Contiune();

		/// <summary>
		/// 退出任务
		/// </summary>
		/// <param name="action">退出任务成功后的回调方法</param>
		void Exit(Action action = null);
	}
}

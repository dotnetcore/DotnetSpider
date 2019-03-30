using System;
using System.Threading;

namespace DotnetSpider.Core
{
	public class ThreadCommonPool
	{
		private int _threadCount;

		public ThreadCommonPool(int maxThreads)
		{
			if (maxThreads <= 0)
			{
				throw new ArgumentException($"{nameof(maxThreads)} should larger than 0.");
			}
			MaxThreads = maxThreads;
		}

		public int MaxThreads { get; }

		/// <summary>
		/// 一直等待直到任务被排上
		/// </summary>
		/// <typeparam name="T">数据对象的类型</typeparam>
		/// <param name="action">任务</param>
		/// <param name="obj">数据对象</param>
		public void QueueUserWork<T>(Action<T> action, T obj)
		{
			if (action == null)
			{
				return;
			}
			while (!TryQueueUserWork(action, obj))
			{
				Thread.Sleep(1);
			}
		}

		/// <summary>
		/// 尝试把任务排到线程池上
		/// </summary>
		/// <typeparam name="T">数据对象类型</typeparam>
		/// <param name="action">任务</param>
		/// <param name="obj">数据对象</param>
		/// <returns>是否排队成功</returns>
		public bool TryQueueUserWork<T>(Action<T> action, T obj)
		{
			if (action == null)
			{
				return false;
			}

			if (_threadCount >= MaxThreads) return false;
			if (!ThreadPool.QueueUserWorkItem(threadContext =>
			{
				try
				{
					action(obj);
				}
				finally
				{
					Interlocked.Decrement(ref _threadCount);
				}
			})) return false;
			Interlocked.Increment(ref _threadCount);
			return true;
		}

		/// <summary>
		/// 一直等待直到任务被排上
		/// </summary>
		/// <param name="action">任务</param>
		public void QueueUserWork(Action action)
		{
			if (action == null)
			{
				return;
			}
			while (!TryQueueUserWork(action))
			{
				Thread.Sleep(1);
			}
		}

		/// <summary>
		/// 尝试把任务排到线程池上
		/// </summary>
		/// <param name="action">任务</param>
		/// <returns>是否排队成功</returns>
		public bool TryQueueUserWork(Action action)
		{
			if (action == null)
			{
				return false;
			}

			if (_threadCount >= MaxThreads) return false;
			if (!ThreadPool.QueueUserWorkItem(threadContext =>
			{
				try
				{
					action();
				}
				finally
				{
					Interlocked.Decrement(ref _threadCount);
				}
			})) return false;
			Interlocked.Increment(ref _threadCount);
			return true;
		}
	}
}
using System;
using System.Diagnostics;
using System.Threading;

namespace DotnetSpider.Proxy
{
	internal class ThreadCommonPool
	{
		delegate void OnThreadExited();

		abstract class Runable
		{
			public abstract void Run();
		}

		class ThreadContext<T> : Runable
		{
			public OnThreadExited OnExited;

			private readonly Action<T> _work;
			private readonly T _obj;

			public ThreadContext(Action<T> action, T obj)
			{
				_work = action;
				_obj = obj;
			}

			public override void Run()
			{
				try
				{
					_work.Invoke(_obj);
				}
				catch (Exception e)
				{
					Debugger.Log(0, "Thread execute failed.", e.Message);
				}
				finally
				{
					OnExited();
				}
			}
		}

		class ThreadContext : Runable
		{
			public OnThreadExited OnExited;

			private readonly Action _work;

			public ThreadContext(Action action)
			{
				_work = action;
			}

			public override void Run()
			{
				try
				{
					_work.Invoke();
				}
				catch (Exception e)
				{
					Debugger.Log(0, "Thread execute failed.", e.Message);
				}
				finally
				{
					OnExited();
				}
			}
		}

		private int _threadNum;

		public ThreadCommonPool(int maxThreadNum)
		{
			if (maxThreadNum <= 0)
			{
				throw new ArgumentException($"{nameof(maxThreadNum)} should larger than 0.");
			}
			MaxThreadNum = maxThreadNum;
		}

		public int MaxThreadNum { get; }

		public int ThreadNum => _threadNum;

		/// <summary>
		/// 一直等待直到任务被排上
		/// </summary>
		/// <typeparam name="T">数据对象的类型</typeparam>
		/// <param name="action">任务</param>
		/// <param name="obj">数据对象</param>
		public void QueueUserWork<T>(Action<T> action, T obj)
		{
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
			if (_threadNum < MaxThreadNum)
			{
				var context = new ThreadContext<T>(action, obj);
				context.OnExited = () =>
				{
					Interlocked.Decrement(ref _threadNum);
				};
				if (ThreadPool.QueueUserWorkItem(threadContext =>
				{
					var runable = (Runable)threadContext;
					runable.Run();
				}, context))
				{
					Interlocked.Increment(ref _threadNum);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// 一直等待直到任务被排上
		/// </summary>
		/// <param name="action">任务</param>
		public void QueueUserWork(Action action)
		{
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
			if (_threadNum < MaxThreadNum)
			{
				var context = new ThreadContext(action);
				context.OnExited = () =>
				{
					Interlocked.Decrement(ref _threadNum);
				};
				if (ThreadPool.QueueUserWorkItem(threadContext =>
				{
					var runable = (Runable)threadContext;
					runable.Run();
				}, context))
				{
					Interlocked.Increment(ref _threadNum);
					return true;
				}
			}
			return false;
		}
	}
}

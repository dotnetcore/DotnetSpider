using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Java2Dotnet.Spider.Common;
#if NET_CORE
using System.Runtime.InteropServices;
#endif

namespace Java2Dotnet.Spider.Core
{
	/// <summary>
	/// Thread pool. 
	/// </summary>
	public class CountableThreadPool
	{
		private bool _exit;
		private AtomicInteger _threadCount = new AtomicInteger(0);

		public CountableThreadPool(int threadNum = 5)
		{
			ThreadNum = threadNum;
			ThreadPool.SetMaxThreads(ThreadNum, ThreadNum);
		}

		public int ThreadAlive
		{
			get
			{
				return _threadCount.Value;
			}
		}

		public int ThreadNum { get; }

		public void Push(Action<object> func, object obj)
		{
			lock (this)
			{
				if (_exit)
				{
					throw new SpiderExceptoin("Pool already exit.");
				}

				while (ThreadAlive >= ThreadNum)
				{
					Thread.Sleep(100);
				}

				_threadCount.Inc();
				ThreadPool.QueueUserWorkItem(new WaitCallback((o) =>
				{
					func(o);
					_threadCount.Dec();
				}), obj);
			}
		}

		public void WaitToExit()
		{
			lock (this)
			{
				while (ThreadAlive > 0)
				{
					Thread.Sleep(500);
				}
				_exit = true;
			}
		}
	}
}
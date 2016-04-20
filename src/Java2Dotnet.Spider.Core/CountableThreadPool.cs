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
		private readonly SynchronizedList<Task> _tasks = new SynchronizedList<Task>();
		private readonly int _cachedSize;
		private bool _exit;

		public CountableThreadPool(int threadNum = 5)
		{
			ThreadNum = threadNum;

			Task.Factory.StartNew(() =>
			{
				while (true)
				{
					if (_exit)
					{
						break;
					}

					var finishedTasks = _tasks.Where(t => t.IsCompleted).ToList();
					foreach (var finishedTask in finishedTasks)
					{
						_tasks.Remove(finishedTask);
					}

					Thread.Sleep(10);
				}
			});
		}

		public int ThreadAlive
		{
			get { return _tasks.Where(t => t.Status == TaskStatus.Running).Count; }
		}

		public int ThreadNum { get; }

		public void Push(Action<object> func, object obj)
		{
			lock (this)
			{
				if (_exit)
				{
					throw new SpiderExceptoin("Pool is exit.");
				}

				while (_tasks.Count() > _cachedSize)
				{
					Thread.Sleep(10);
				}

				Task task = Task.Factory.StartNew((o) => { func(o); }, obj);
				_tasks.Add(task);
			}
		}

		public void WaitToExit()
		{
			lock (this)
			{
				Task.WaitAll(_tasks.GetAll().ToArray());
				_exit = true;
			}
		}
	}
}
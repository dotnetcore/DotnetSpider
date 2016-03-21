//using System.Collections.Generic;
//using System.Runtime.CompilerServices;

//namespace Java2Dotnet.Spider.Core.Scheduler
//{
//	/// <summary>
//	/// Basic Scheduler implementation. 
//	/// Store urls to fetch in LinkedBlockingQueue and remove duplicate urls by HashMap.
//	/// </summary>
//	//check:
//	//[Synchronization]
//	public class QueueScheduler : IScheduler, IMonitorableScheduler
//	{
//		//check:
//		private readonly Queue<Request> _queue = new Queue<Request>();
//		private readonly List<Request> _allRequests = new List<Request>();

//		public void Init(ISpider spider)
//		{
//		}

//		[MethodImpl(MethodImplOptions.Synchronized)]
//		public void Push(Request request, ISpider spider)
//		{
//			_queue.Enqueue(request);
//			_allRequests.Add(request);
//		}

//		[MethodImpl(MethodImplOptions.Synchronized)]
//		public Request Poll(ISpider spider)
//		{
//			return _queue.Count > 0 ? _queue.Dequeue() : null;
//		}

//		public int GetLeftRequestsCount(ISpider spider)
//		{
//			return _queue.Count;
//		}

//		public int GetTotalRequestsCount(ISpider spider)
//		{
//			return _allRequests.Count;
//		}
//	}
//}
using DotnetSpider.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotnetSpider.Core.Scheduler
{
	public class BlockQueueScheduler : QueueDuplicateRemovedScheduler
	{
		private readonly string _service;
		private readonly string _identity;
		private Task _uploadTask;
		private bool _disposing;

		public int BlockSize { get; set; } = 100;

		public BlockQueueScheduler(string service, string identity)
		{
			_service = $"{new Uri(service).ToString()}requestqueue/{identity}";
			_identity = identity;
		}

		public override Request Poll()
		{
			lock (_lock)
			{
				if (_uploadTask == null)
				{
					_uploadTask = Task.Factory.StartNew(() =>
					{
						while (!_disposing)
						{
							PullAndPush();
							Thread.Sleep(60000);
						}
					});
				}

				if (_queue.Count > 0)
				{
					Request request = _queue.First();
					_queue.RemoveAt(0);
					return request;
				}
			}
			return null;
		}

		public override void Dispose()
		{
			lock (_lock)
			{
				_disposing = true;
				PullAndPush();
			}
			base.Dispose();
		}

		private IEnumerable<Request> GetRequests()
		{
			var json = DotnetSpider.Downloader.Downloader.Default.GetAsync(_service).Result.Content.ReadAsStringAsync().Result;
			return JsonConvert.DeserializeObject<List<Request>>(json);
		}

		private void PullAndPush()
		{
			if (_queue.Count < 5)
			{
				 
			}
		}
	}
}

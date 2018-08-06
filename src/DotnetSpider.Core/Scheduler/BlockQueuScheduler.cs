using DotnetSpider.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DotnetSpider.Core.Scheduler
{
	public class BlockQueueScheduler : QueueDuplicateRemovedScheduler
	{
		private readonly string _uploadUrl;
		private bool _uploaded;

		public int BlockSize { get; set; } = 100;

		public BlockQueueScheduler(string service)
		{
			_uploadUrl = $"{new Uri(service).ToString()}block/";
		}

		public override Request Poll()
		{
			lock (_lock)
			{
				if (!_uploaded)
				{
					Load();
					_uploaded = true;
					_queue.Clear();
				}

				if (_queue.Count == 0)
				{
					_queue.AddRange(Download());
					Thread.Sleep(5000);
				}
				else
				{
					Request request = _queue.First();
					_queue.RemoveAt(0);
					return request;
				}
			}
			return null;
		}

		private IEnumerable<Request> Download()
		{
			return null;
		}

		private void Load()
		{
		}
	}
}

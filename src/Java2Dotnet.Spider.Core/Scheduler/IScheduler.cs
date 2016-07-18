using System;
using System.Collections.Generic;

namespace Java2Dotnet.Spider.Core.Scheduler
{
	/// <summary>
	/// Scheduler is the part of url management. 
	/// You can implement interface Scheduler to do:
	/// manage urls to fetch
	/// remove duplicate urls
	/// </summary>
	public interface IScheduler : IDisposable
	{
		void Init(ISpider spider);

		ISpider Spider { get; }

		/// <summary>
		/// Add a url to fetch
		/// </summary>
		/// <param name="request"></param>
		/// <param name="spider"></param>
		void Push(Request request);

		/// <summary>
		/// Get an url to crawl
		/// </summary>
		/// <param name="spider"></param>
		/// <returns></returns>
		Request Poll();

		void Load(HashSet<Request> requests);

		HashSet<Request> ToList();
	}
}

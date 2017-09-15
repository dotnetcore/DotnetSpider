using System;
using System.Collections.Generic;
using System.Net;
using DotnetSpider.Core.Selector;
using DotnetSpider.Core.Infrastructure;

namespace DotnetSpider.Core
{
	/// <summary>
	/// Object storing extracted result and urls to fetch. 
	/// </summary>
	public class Page
	{
		private readonly object _locker = new object();
		private Selectable _selectable;
		private string _content;

		public ContentType ContentType { get; internal set; } = ContentType.Html;

		/// <summary>
		/// Url of current page
		/// </summary>
		/// <returns></returns>
		public string Url { get; }

		/// <summary>
		/// Get url of current page
		/// </summary>
		/// <returns></returns>
		public string TargetUrl { get; set; }

		/// <summary>
		/// Title of current page.
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Get request of current page
		/// </summary>
		/// <returns></returns>
		public Request Request { get; }

		/// <summary>
		/// Whether need retry current page.
		/// </summary>
		public bool Retry { get; set; }

		/// <summary>
		/// Skip extract target urls, when someone use custom target url builder.
		/// </summary>
		public bool SkipExtractTargetUrls { get; set; }

		/// <summary>
		/// Skip all target urls, will not add to scheduler.
		/// </summary>
		public bool SkipTargetUrls { get; set; }

		/// <summary>
		/// Skip current page.
		/// </summary>
		public bool Skip { get; set; }

		public ResultItems ResultItems { get; } = new ResultItems();

		public HttpStatusCode? StatusCode => Request?.StatusCode;

		public string Padding { get; set; }

		public string Content
		{
			get => _content;
			set
			{
				if (!Equals(value, _content))
				{
					_content = value;
					_selectable = null;
				}
			}
		}

		public Exception Exception { get; set; }

		public HashSet<Request> TargetRequests { get; } = new HashSet<Request>();

		/// <summary>
		/// Whether remove outbound urls.
		/// </summary>
		public bool RemoveOutboundLinks { get; }

		/// <summary>
		/// Only used to remove outbound urls.
		/// </summary>
		public string[] Domains { get; }

		/// <summary>
		/// Get selectable interface
		/// </summary>
		/// <returns></returns>
		public Selectable Selectable
		{
			get
			{
				if (_selectable == null)
				{
					string urlPadding = ContentType == ContentType.Json ? Padding : Request.Url.ToString();
					_selectable = new Selectable(Content, urlPadding, ContentType, RemoveOutboundLinks ? Domains : null);
				}
				return _selectable;
			}
		}

		public Page(Request request, params string[] domains)
		{
			Request = request;
			Url = request.Url.ToString();
			ResultItems.Request = request;
			RemoveOutboundLinks = domains != null && domains.Length > 0;
			Domains = domains;
		}

		/// <summary>
		/// Store extract results
		/// </summary>
		/// <param name="key"></param>
		/// <param name="field"></param>
		public void AddResultItem(string key, dynamic field)
		{
			ResultItems.AddOrUpdateResultItem(key, field);
		}

		/// <summary>
		/// Add urls to fetch
		/// </summary>
		/// <param name="requests"></param>
		public void AddTargetRequests(IList<string> requests)
		{
			if (requests == null || requests.Count == 0)
			{
				return;
			}
			lock (_locker)
			{
				foreach (string s in requests)
				{
					if (string.IsNullOrEmpty(s) || s.Equals("#") || s.StartsWith("javascript:"))
					{
						continue;
					}
					string s1 = UrlUtils.CanonicalizeUrl(s, Url);
					var request = new Request(s1, Request.Extras) { Depth = Request.NextDepth };
					if (request.IsAvailable)
					{
						TargetRequests.Add(request);
					}
				}
			}
		}

		/// <summary>
		/// Add urls to fetch
		/// </summary>
		/// <param name="requests"></param>
		public void AddTargetRequests(IList<Request> requests)
		{
			if (requests == null || requests.Count == 0)
			{
				return;
			}
			lock (_locker)
			{
				foreach (var r in requests)
				{
					r.Depth = Request.NextDepth;
					TargetRequests.Add(r);
				}
			}
		}

		/// <summary>
		/// Add urls to fetch
		/// </summary>
		/// <param name="requests"></param>
		/// <param name="priority"></param>
		public void AddTargetRequests(IList<string> requests, int priority)
		{
			if (requests == null || requests.Count == 0)
			{
				return;
			}
			lock (_locker)
			{
				foreach (string s in requests)
				{
					if (string.IsNullOrEmpty(s) || s.Equals("#") || s.StartsWith("javascript:"))
					{
						continue;
					}
					string s1 = UrlUtils.CanonicalizeUrl(s, Url);
					Request request = new Request(s1, Request.Extras) { Priority = priority, Depth = Request.NextDepth };
					if (request.IsAvailable)
					{
						TargetRequests.Add(request);
					}
				}
			}
		}

		/// <summary>
		/// Add url to fetch
		/// </summary>
		/// <param name="requestString"></param>
		/// <param name="increaseDeep"></param>
		public void AddTargetRequest(string requestString, bool increaseDeep = true)
		{
			lock (_locker)
			{
				if (string.IsNullOrEmpty(requestString) || requestString.Equals("#"))
				{
					return;
				}

				requestString = UrlUtils.CanonicalizeUrl(requestString, Url);
				var request = new Request(requestString, Request.Extras)
				{
					Depth = Request.NextDepth
				};

				if (increaseDeep)
				{
					request.Depth = Request.NextDepth;
				}
				TargetRequests.Add(request);
			}
		}

		/// <summary>
		/// Add requests to fetch
		/// </summary>		 
		public void AddTargetRequest(Request request, bool increaseDeep = true)
		{
			if (request == null)
			{
				return;
			}
			lock (_locker)
			{
				if (request.IsAvailable)
				{
					if (increaseDeep)
					{
						request.Depth = Request.NextDepth;
					}
					TargetRequests.Add(request);
				}
			}
		}
	}
}

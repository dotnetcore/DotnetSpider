using System;
using System.Collections.Generic;
using DotnetSpider.Core.Selector;
using DotnetSpider.Core.Common;

namespace DotnetSpider.Core
{
	/// <summary>
	/// Object storing extracted result and urls to fetch. 
	/// </summary>
	public class Page
	{
		public const string Images = "580c9065-0f44-47e9-94ea-b172d5a730c0";

		private Selectable _selectable;

		private string _content;

		/// <summary>
		/// Url of current page
		/// </summary>
		/// <returns></returns>
		public string Url { get; set; }

		/// <summary>
		/// Get url of current page
		/// </summary>
		/// <returns></returns>
		public string TargetUrl { get; set; }

		public string Title { get; set; }

		public ContentType ContentType { get; set; }

		/// <summary>
		/// Get request of current page
		/// </summary>
		/// <returns></returns>
		public Request Request { get; }

		public bool IsNeedCycleRetry { get; set; }

		public ResultItems ResultItems { get; } = new ResultItems();

		public int StatusCode { get; set; }

		public string Padding { get; set; }

		public string Content
		{
			get { return _content; }
			set
			{
				if (!Equals(value, _content))
				{
					_content = value;
					_selectable = null;
				}
			}
		}

		public bool MissTargetUrls { get; set; }

		public bool IsSkip
		{
			get { return ResultItems.IsSkip; }
			set { ResultItems.IsSkip = value; }
		}

		public Exception Exception { get; set; }

		public HashSet<Request> TargetRequests { get; } = new HashSet<Request>();

		public Page(Request request, ContentType contentType)
		{
			Request = request;
			ResultItems.Request = request;
			ContentType = contentType;
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
		/// Get html content of page
		/// </summary>
		/// <returns></returns>
		public Selectable Selectable => _selectable ?? (_selectable = new Selectable(Content, ContentType == ContentType.Json ? Padding : Request.Url.ToString(), ContentType));

		/// <summary>
		/// Add urls to fetch
		/// </summary>
		/// <param name="requests"></param>
		public void AddTargetRequests(IList<string> requests)
		{
			lock (this)
			{
				foreach (string s in requests)
				{
					if (string.IsNullOrEmpty(s) || s.Equals("#") || s.StartsWith("javascript:"))
					{
						continue;
					}
					string s1 = UrlUtils.CanonicalizeUrl(s, Url);
					TargetRequests.Add(new Request(s1, Request.NextDepth, Request.Extras));
				}
			}
		}

		/// <summary>
		/// Add urls to fetch
		/// </summary>
		/// <param name="requests"></param>
		public void AddTargetRequests(IList<Request> requests)
		{
			lock (this)
			{
				foreach (var r in requests)
				{
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
			lock (this)
			{
				foreach (string s in requests)
				{
					if (string.IsNullOrEmpty(s) || s.Equals("#") || s.StartsWith("javascript:"))
					{
						continue;
					}
					string s1 = UrlUtils.CanonicalizeUrl(s, Url);
					Request request = new Request(s1, Request.NextDepth, Request.Extras) { Priority = priority };
					TargetRequests.Add(request);
				}
			}
		}

		/// <summary>
		/// Add url to fetch
		/// </summary>
		/// <param name="requestString"></param>
		public void AddTargetRequest(string requestString)
		{
			lock (this)
			{
				if (string.IsNullOrEmpty(requestString) || requestString.Equals("#"))
				{
					return;
				}

				requestString = UrlUtils.CanonicalizeUrl(requestString, Url);
				TargetRequests.Add(new Request(requestString, Request.NextDepth, Request.Extras));
			}
		}

		/// <summary>
		/// Add requests to fetch
		/// </summary>		 
		public void AddTargetRequest(Request request)
		{
			lock (this)
			{
				TargetRequests.Add(request);
			}
		}

		public override string ToString()
		{
			return $"Page{{request='{Request}',padding='{Padding}' resultItems='{ResultItems}', content='{Content}', url={Url}, statusCode={StatusCode}, targetRequests={TargetRequests}}}";
		}
	}
}

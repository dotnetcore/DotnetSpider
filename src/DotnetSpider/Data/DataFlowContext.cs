using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using DotnetSpider.Core;
using DotnetSpider.Data.Parser;
using DotnetSpider.Downloader;
using DotnetSpider.Selector;

namespace DotnetSpider.Data
{
	/// <summary>
	/// 数据流处理器上下文
	/// </summary>
	public class DataFlowContext
	{
		private readonly Dictionary<string, dynamic> _properties = new Dictionary<string, dynamic>();
		private readonly Dictionary<string, dynamic> _items = new Dictionary<string, dynamic>();
		private readonly Dictionary<string, IParseResult> _parseItems = new Dictionary<string, IParseResult>();
		private readonly List<Request> _followRequests = new List<Request>();
		private ISelectable _selectable;

		public IServiceProvider Services { get; }

		public Response Response { get; }

		internal List<Request> FollowRequests => _followRequests;

		public DataFlowContext(Response response, IServiceProvider serviceProvider)
		{
			Response = response;
			Services = serviceProvider;
		}

		public ISelectable GetSelectable(ContentType contentType = ContentType.Auto, bool removeOutboundLinks = true)
		{
			return _selectable ?? (_selectable = Response?.ToSelectable(contentType, removeOutboundLinks));
		}

		public string Result { get; set; }

		public dynamic this[string key]
		{
			get => _properties.ContainsKey(key) ? _properties[key] : null;
			set
			{
				if (_properties.ContainsKey(key))
				{
					_properties[key] = value;
				}

				else
				{
					_properties.Add(key, value);
				}
			}
		}

		public bool Contains(string key)
		{
			return _properties.ContainsKey(key);
		}

		public void Add(string key, dynamic value)
		{
			_properties.Add(key, value);
		}

		public void AddItem(string name, dynamic value)
		{
			if (_items.ContainsKey(name))
			{
				_items[name] = value;
			}

			else
			{
				_items.Add(name, value);
			}
		}

		public dynamic GetItem(string name)
		{
			return _items.ContainsKey(name) ? _items[name] : null;
		}

		public IDictionary<string, dynamic> GetItems()
		{
			return _items.ToImmutableDictionary();
		}

		public void AddParseItem(string name, IParseResult value)
		{
			if (_parseItems.ContainsKey(name))
			{
				_parseItems[name] = value;
			}

			else
			{
				_parseItems.Add(name, value);
			}
		}

		public IParseResult GetParseItem(string name)
		{
			return _parseItems.ContainsKey(name) ? _parseItems[name] : null;
		}

		public IDictionary<string, IParseResult> GetParseItems()
		{
			return _parseItems.ToImmutableDictionary();
		}

		public bool HasItems => _items != null && _items.Count > 0;

		public bool HasParseItems => _parseItems != null && _parseItems.Count > 0;

		public void ClearItems()
		{
			_items.Clear();
		}
	}
}
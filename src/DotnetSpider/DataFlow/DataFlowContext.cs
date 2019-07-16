using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using DotnetSpider.Common;
using DotnetSpider.DataFlow.Parser;
using DotnetSpider.Downloader;
using DotnetSpider.Selector;

namespace DotnetSpider.DataFlow
{
	/// <summary>
	/// 数据流处理器上下文
	/// </summary>
	public class DataFlowContext
	{
		private readonly Dictionary<string, dynamic> _properties = new Dictionary<string, dynamic>();
		private readonly Dictionary<string, dynamic> _items = new Dictionary<string, dynamic>();
		private readonly Dictionary<string, IParseResult> _parseItems = new Dictionary<string, IParseResult>();
		private ISelectable _selectable;

		/// <summary>
		/// 注入的服务
		/// </summary>
		public IServiceProvider Services { get; }

		/// <summary>
		/// 下载器返回的结果
		/// </summary>
		public Response Response { get; }

		/// <summary>
		/// 解析到的目标链接
		/// </summary>
		public List<Request> FollowRequests { get; set; } = new List<Request>();

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="response">下载器返回的结果</param>
		/// <param name="serviceProvider">注入的服务</param>
		public DataFlowContext(Response response, IServiceProvider serviceProvider)
		{
			Response = response;
			Services = serviceProvider;
		}

		/// <summary>
		/// 获取查询器
		/// </summary>
		/// <param name="contentType">内容类型</param>
		/// <param name="removeOutboundLinks">是否删除外链</param>
		/// <returns></returns>
		public ISelectable GetSelectable(ContentType contentType = ContentType.Auto, bool removeOutboundLinks = true)
		{
			return _selectable ?? (_selectable = Response?.ToSelectable(contentType, removeOutboundLinks));
		}

		/// <summary>
		/// 数据流处理结果
		/// </summary>
		public string Result { get; set; }

		/// <summary>
		/// 获取属性
		/// </summary>
		/// <param name="key">Key</param>
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

		/// <summary>
		/// 是否包含属性
		/// </summary>
		/// <param name="key">Key</param>
		/// <returns></returns>
		public bool Contains(string key)
		{
			return _properties.ContainsKey(key);
		}

		/// <summary>
		/// 添加属性
		/// </summary>
		/// <param name="key">Key</param>
		/// <param name="value">Value</param>
		public void Add(string key, dynamic value)
		{
			_properties.Add(key, value);
		}

		/// <summary>
		/// 添加数据项
		/// </summary>
		/// <param name="name">Name</param>
		/// <param name="value">Value</param>
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

		/// <summary>
		/// 获取数据项
		/// </summary>
		/// <param name="name">Name</param>
		/// <returns></returns>
		public dynamic GetItem(string name)
		{
			return _items.ContainsKey(name) ? _items[name] : null;
		}

		/// <summary>
		/// 获取所有数据项
		/// </summary>
		/// <returns></returns>
		public IDictionary<string, dynamic> GetItems()
		{
			return _items.ToImmutableDictionary();
		}

		/// <summary>
		/// 添加实体解析结果
		/// </summary>
		/// <param name="name">Name</param>
		/// <param name="value">实体结构结果</param>
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

		/// <summary>
		/// 获取实体结析结果项
		/// </summary>
		/// <param name="name">Name</param>
		/// <returns></returns>
		public IParseResult GetParseItem(string name)
		{
			return _parseItems.ContainsKey(name) ? _parseItems[name] : null;
		}

		/// <summary>
		/// 获取实体结析结果项
		/// </summary>
		/// <returns></returns>
		public IDictionary<string, IParseResult> GetParseItems()
		{
			return _parseItems.ToImmutableDictionary();
		}

		/// <summary>
		/// 是否包含数据项
		/// </summary>
		public bool HasItems => _items != null && _items.Count > 0;

		/// <summary>
		/// 是否包含实体结析结果项
		/// </summary>
		public bool HasParseItems => _parseItems != null && _parseItems.Count > 0;

		/// <summary>
		/// 清空数据项
		/// </summary>
		public void ClearItems()
		{
			_items.Clear();
		}
	}
}
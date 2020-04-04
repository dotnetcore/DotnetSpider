using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DotnetSpider.Http;
using DotnetSpider.Selector;

namespace DotnetSpider.DataFlow
{
	/// <summary>
	/// 数据流处理器上下文
	/// </summary>
	public class DataContext
	{
		private readonly Dictionary<string, dynamic> _properties = new Dictionary<string, dynamic>();
		private readonly Dictionary<object, dynamic> _data = new Dictionary<object, dynamic>();

		public ISelectable Selectable { get; internal set; }

		public SpiderOptions Options { get; }

		/// <summary>
		/// 下载器返回的结果
		/// </summary>
		public Response Response { get; }

		public Request Request { get; }

		/// <summary>
		/// 解析到的目标链接
		/// </summary>
		internal List<Request> FollowRequests { get; }

		public IServiceProvider ServiceProvider { get; }

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="request"></param>
		/// <param name="response">下载器返回的结果</param>
		/// <param name="options"></param>
		/// <param name="serviceProvider"></param>
		public DataContext(IServiceProvider serviceProvider,
			SpiderOptions options,
			Request request,
			Response response
		)
		{
			Request = request;
			Response = response;
			Options = options;
			ServiceProvider = serviceProvider;
			FollowRequests = new List<Request>();
		}

		public void AddFollowRequests(params Request[] requests)
		{
			if (requests != null)
			{
				FollowRequests.AddRange(requests);
			}
		}

		public void AddFollowRequests(IEnumerable<Request> requests)
		{
			if (requests != null)
			{
				FollowRequests.AddRange(requests);
			}
		}

		public void AddFollowRequests(IEnumerable<string> uris)
		{
			if (uris != null)
			{
				FollowRequests.AddRange(uris.Select(CreateNewRequest));
			}
		}

		public Request CreateNewRequest(string uri)
		{
			return Request.CreateNew(uri);
		}

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
		/// <param name="data">Value</param>
		public void AddData(object name, dynamic data)
		{
			if (_data.ContainsKey(name))
			{
				_data[name] = data;
			}

			else
			{
				_data.Add(name, data);
			}
		}

		/// <summary>
		/// 获取数据项
		/// </summary>
		/// <param name="name">Name</param>
		/// <returns></returns>
		public dynamic GetData(object name)
		{
			return _data.ContainsKey(name) ? _data[name] : null;
		}

		/// <summary>
		/// 获取所有数据项
		/// </summary>
		/// <returns></returns>
		public IDictionary<object, dynamic> GetData()
		{
			return _data.ToImmutableDictionary();
		}

		/// <summary>
		/// 是否包含数据项
		/// </summary>
		public bool IsEmpty => _data.Count == 0;

		/// <summary>
		/// 清空数据
		/// </summary>
		public void Clear()
		{
			_data.Clear();
		}
	}
}

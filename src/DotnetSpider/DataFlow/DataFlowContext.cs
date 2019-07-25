using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
		private readonly Dictionary<string, dynamic> _data = new Dictionary<string, dynamic>();
		private readonly Dictionary<string, IParseResult> _parseData = new Dictionary<string, IParseResult>();

		public ISelectable Selectable { get; internal set; }

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
		internal List<Request> FollowRequests { get; set; } = new List<Request>();

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

		public void AddFollowRequests(params Request[] requests)
		{
			if (requests != null && requests.Length > 0)
			{
				FollowRequests.AddRange(requests);
			}
		}

		/// <summary>
		/// 数据流处理结果
		/// </summary>
		public string Message { get; set; }

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
		public void AddData(string name, dynamic data)
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
		public dynamic GetData(string name)
		{
			return _data.ContainsKey(name) ? _data[name] : null;
		}

		/// <summary>
		/// 获取所有数据项
		/// </summary>
		/// <returns></returns>
		public IDictionary<string, dynamic> GetData()
		{
			return _data.ToImmutableDictionary();
		}

		/// <summary>
		/// 添加实体解析结果
		/// </summary>
		/// <param name="name">Name</param>
		/// <param name="value">实体结构结果</param>
		public void AddParseData(string name, IParseResult value)
		{
			if (_parseData.ContainsKey(name))
			{
				_parseData[name] = value;
			}

			else
			{
				_parseData.Add(name, value);
			}
		}

		/// <summary>
		/// 获取实体结析结果项
		/// </summary>
		/// <param name="name">Name</param>
		/// <returns></returns>
		public IParseResult GetParseData(string name)
		{
			return _parseData.ContainsKey(name) ? _parseData[name] : null;
		}

		/// <summary>
		/// 获取实体结析结果项
		/// </summary>
		/// <returns></returns>
		public IDictionary<string, IParseResult> GetParseData()
		{
			return _parseData.ToImmutableDictionary();
		}

		/// <summary>
		/// 是否包含数据项
		/// </summary>
		public bool HasData => _data != null && _data.Count > 0;

		/// <summary>
		/// 是否包含实体结析结果项
		/// </summary>
		public bool HasParseData => _parseData != null && _parseData.Count > 0;

		/// <summary>
		/// 清空数据项
		/// </summary>
		public void ClearData()
		{
			_data.Clear();
		}
	}
}
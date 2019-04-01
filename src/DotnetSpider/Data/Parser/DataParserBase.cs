using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotnetSpider.Downloader;
using DotnetSpider.Selector;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.Data.Parser
{
	/// <summary>
	/// 数据解析器
	/// </summary>
	public abstract class DataParserBase : DataFlowBase
	{
		/// <summary>
		/// 判断当前请求是否可以解析
		/// </summary>
		public Func<Request, bool> CanParse { get; set; }

		/// <summary>
		/// 查询当前请求的下一级请求
		/// </summary>
		public Func<DataFlowContext, List<string>> QueryFollowRequests { get; set; }

		/// <summary>
		/// 选择器的生成方法
		/// </summary>
		public Func<DataFlowContext, ISelectable> SelectableFactory { get; set; }

		/// <summary>
		/// 数据解析
		/// </summary>
		/// <param name="context">处理上下文</param>
		/// <returns></returns>
		public override async Task<DataFlowResult> HandleAsync(DataFlowContext context)
		{
			if (context?.Response == null)
			{
				Logger?.LogError("数据上下文或者响应内容为空");
				return DataFlowResult.Failed;
			}

			try
			{
				// 如果不匹配则跳过，不影响其它数据流处理器的执行
				if (CanParse != null && !CanParse(context.Response.Request))
				{
					return DataFlowResult.Success;
				}

				SelectableFactory?.Invoke(context);

				var parserResult = await Parse(context);

				var urls = QueryFollowRequests?.Invoke(context);
				AddTargetRequests(context, urls);

				if (parserResult == DataFlowResult.Failed || parserResult == DataFlowResult.Terminated)
				{
					return parserResult;
				}

				return DataFlowResult.Success;
			}
			catch (Exception e)
			{
				Logger?.LogError($"任务 {context.Response.Request.OwnerId} 数据解析发生异常: {e}");
				return DataFlowResult.Failed;
			}
		}

		protected virtual void AddTargetRequests(DataFlowContext dfc, List<string> urls)
		{
			if (urls != null && urls.Count > 0)
			{
				var followRequests = new List<Request>();
				foreach (var url in urls)
				{
					var followRequest = CreateFromRequest(dfc.Response.Request, url);
					if (CanParse == null || CanParse(followRequest))
					{
						followRequests.Add(followRequest);
					}
				}

				dfc.FollowRequests.AddRange(followRequests.ToArray());
			}
		}

		protected virtual Request CreateFromRequest(DataFlowContext dfc, string url)
		{
			return CreateFromRequest(dfc.Response.Request, url);
		}

		protected virtual IEnumerable<Request> CreateFromRequests(DataFlowContext dfc, IEnumerable<string> urls)
		{
			return CreateFromRequests(dfc.Response.Request, urls);
		}

		/// <summary>
		/// 创建当前请求的下一级请求
		/// </summary>
		/// <param name="current">当前请求</param>
		/// <param name="url">下一级请求</param>
		/// <returns></returns>
		protected virtual Request CreateFromRequest(Request current, string url)
		{
			// TODO: 确认需要复制哪些字段
			var request = new Request(url, current.GetProperties())
			{
				Url = url,
				Depth = current.Depth,
				Body = current.Body,
				Method = current.Method,
				AgentId = current.AgentId,
				RetriedTimes = 0,
				OwnerId = current.OwnerId
			};
			return request;
		}

		protected virtual IEnumerable<Request> CreateFromRequests(Request current, IEnumerable<string> urls)
		{
			var list = new List<Request>();
			foreach (var url in urls)
			{
				list.Add(CreateFromRequest(current, url));
			}

			return list;
		}

		/// <summary>
		/// 数据解析
		/// </summary>
		/// <param name="context">处理上下文</param>
		/// <returns></returns>
		protected abstract Task<DataFlowResult> Parse(DataFlowContext context);
	}
}
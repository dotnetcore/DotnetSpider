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
        public Func<DataFlowContext, string[]> Follow { get; set; }

        /// <summary>
        /// 当前请求的内容构造的选择器
        /// </summary>
        public Func<DataFlowContext, ISelectable> Selectable { get; set; }

        /// <summary>
        /// 数据解析
        /// </summary>
        /// <param name="context">处理上下文</param>
        /// <returns></returns>
        public override async Task<DataFlowResult> HandleAsync(DataFlowContext context)
        {
            try
            {
                if (context.Response == null)
                {
                    Logger?.LogError("数据上下文中未包含响应内容");
                    return DataFlowResult.Failed;
                }

                // 如果不匹配则跳过，不影响其它数据流处理器的执行
                if (CanParse != null && !CanParse(context.Response.Request))
                {
                    return DataFlowResult.Success;
                }

                Selectable?.Invoke(context);

                var parserResult = await Parse(context);
                if (parserResult == DataFlowResult.Failed || parserResult == DataFlowResult.Terminated)
                {
                    return parserResult;
                }

                var urls = Follow?.Invoke(context);
                if (urls != null && urls.Length > 0)
                {
                    var followRequests = new List<Request>();
                    foreach (var url in urls)
                    {
                        var followRequest = CreateFromRequest(context.Response.Request, url);
                        if (CanParse(followRequest))
                        {
                            followRequests.Add(followRequest);
                        }
                    }

                    context.FollowRequests.AddRange(followRequests.ToArray());
                }

                return DataFlowResult.Success;
            }
            catch (Exception e)
            {
                Logger?.LogError($"数据解析发生异常: {e}");
                return DataFlowResult.Failed;
            }
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
            var request = new Request(url, current.Properties)
                {Url = url, Depth = current.Depth, Body = current.Body, Method = current.Method};
            request.AgentId = current.AgentId;
            request.RetriedTimes = 0;
            request.OwnerId = current.OwnerId;
            return request;
        }

        /// <summary>
        /// 数据解析
        /// </summary>
        /// <param name="context">处理上下文</param>
        /// <returns></returns>
        protected abstract Task<DataFlowResult> Parse(DataFlowContext context);
    }
}
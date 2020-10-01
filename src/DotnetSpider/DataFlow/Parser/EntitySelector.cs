using System;

namespace DotnetSpider.DataFlow.Parser
{
    /// <summary>
    /// 实体选择器
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class EntitySelector : Selector
    {
        /// <summary>
        /// 从最终解析到的结果中取前 Take 个实体
        /// </summary>
        public int Take { get; set; }

        /// <summary>
        /// 设置 Take 的方向, 默认是从头部取
        /// </summary>
        public bool TakeByDescending { get; set; }
    }
}

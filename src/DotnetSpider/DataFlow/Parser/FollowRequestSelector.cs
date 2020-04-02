using System;
using Newtonsoft.Json;

namespace DotnetSpider.DataFlow.Parser
{
    /// <summary>
    /// 目标链接选择器的定义
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class FollowRequestSelector : System.Attribute
    {
#if !NET451
        /// <summary>
        /// 避免被序列化出去
        /// </summary>
        [JsonIgnore]
        public override object TypeId => base.TypeId;
#endif
        public FollowRequestSelector()
        {
        }

        public FollowRequestSelector(string[] xPaths, string[] patterns = null)
        {
            XPaths = xPaths;
            Patterns = patterns;
        }

        public FollowRequestSelector(string xpath)
        {
            XPaths = new[] {xpath};
        }

        public FollowRequestSelector(string xpath, string pattern)
        {
            XPaths = new[] {xpath};
            Patterns = new[] {pattern};
        }

        /// <summary>
        /// 目标链接所在区域
        /// </summary>
        public string[] XPaths { get; set; } = new string[0];

        /// <summary>
        /// 匹配目标链接的正则表达式
        /// </summary>
        public string[] Patterns { get; set; } = new string[0];
    }
}
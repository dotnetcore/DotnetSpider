using System;
using System.Collections.Generic;

namespace DotnetSpider.Http
{
    [Serializable]
    public class ResponseContent
    {
        /// <summary>
        /// Headers
        /// </summary>
        public Dictionary<string, HashSet<string>> Headers { get; set; } = new Dictionary<string, HashSet<string>>();

        /// <summary>
        /// 内容
        /// </summary>
        public byte[] Data { get; set; }
    }
}
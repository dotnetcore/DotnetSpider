using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DotnetSpider.Http
{
    [Serializable]
    public class Response
    {
        public string Agent { get; set; }

        /// <summary>
        /// Request
        /// </summary>
        public string RequestHash { get; set; }

        /// <summary>
        /// 返回状态码
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Headers
        /// </summary>
        public Dictionary<string, HashSet<string>> Headers { get; set; } = new Dictionary<string, HashSet<string>>();

        /// <summary>
        /// 下载内容
        /// </summary>
        public ResponseContent Content { get; set; }

        /// <summary>
        /// 下载消耗的时间
        /// </summary>
        public int ElapsedMilliseconds { get; set; }

        /// <summary>
        /// 最终地址
        /// </summary>
        public string TargetUri { get; set; }

        public string ReadAsString(Encoding encoding = default)
        {
            var coding = encoding ?? Encoding.UTF8;
            return coding.GetString(Content.Data);
        }
    }
}
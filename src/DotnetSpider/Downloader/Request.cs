using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net.Http;
using DotnetSpider.Core;

namespace DotnetSpider.Downloader
{
    public class Request
    {
        private readonly Dictionary<string, string> _properties = new Dictionary<string, string>();

        public string Hash { get; set; }

        public string OwnerId { get; set; }

        public string AgentId { get; set; }

        /// <summary>
        /// 链接的深度，用户不得修改
        /// </summary>
        public int Depth { get; set; }

        public string Url { get; set; }
        
        public string ChangeIpPattern { get; set; }

        #region Headers

        /// <summary>
        /// User-Agent
        /// </summary>
        public string UserAgent { get; set; } =
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.81 Safari/537.36";

        /// <summary>
        /// 请求链接时Referer参数的值
        /// </summary>
        public string Referer { get; set; }

        /// <summary>
        /// 请求链接时Origin参数的值
        /// </summary>
        public string Origin { get; set; }

        /// <summary>
        /// Accept
        /// </summary>
        public string Accept { get; set; }

        /// <summary>
        /// 仅在发送 POST 请求时需要设置
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// 设置 Cookie
        /// </summary>
        public string Cookie { get; set; }

        /// <summary>
        /// Headers
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        #endregion

        /// <summary>
        /// 字符编码
        /// </summary>
        public string Encoding { get; set; }

        public string Body { get; set; }

        public HttpMethod Method { get; set; } = HttpMethod.Get;

        public int RetriedTimes { get; set; }

        public Compression Compression { get; set; }

        public IDictionary<string, string> Properties => _properties.ToImmutableDictionary();

        /// <summary>
        /// 构造方法
        /// </summary>
        public Request()
        {
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="url">链接</param>
        public Request(string url) : this(url, null)
        {
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="url">链接</param>
        /// <param name="properties">额外属性</param>
        public Request(string url, IDictionary<string, string> properties = null)
        {
            Url = url;
            AddProperty(properties);
        }

        /// <summary>
        /// 设置此链接的额外信息
        /// </summary>
        /// <param name="key">键值</param>
        /// <param name="value">额外信息</param>
        public void AddProperty(string key, string value)
        {
            if (null == key)
            {
                return;
            }

            if (_properties.ContainsKey(key))
            {
                _properties[key] = value;
            }
            else
            {
                _properties.Add(key, value);
            }
        }

        public void AddProperty(IDictionary<string, string> dict)
        {
            if (null == dict)
            {
                return;
            }

            foreach (var kv in dict)
            {
                AddProperty(kv.Key, kv.Value);
            }
        }

        public string GetProperty(string key)
        {
            return _properties.ContainsKey(key) ? _properties[key] : null;
        }

        /// <summary>
        /// 设置请求头
        /// </summary>
        /// <param name="key">请求头</param>
        /// <param name="value">请求值</param>
        public void AddHeader(string key, string value)
        {
            if (Headers.ContainsKey(key))
            {
                Headers[key] = value;
            }
            else
            {
                Headers.Add(key, value);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Properties.Clear();
        }

        /// <summary>
        /// Hash 放在客户端计算的原因是调度器可能是分布式的。不能随意去调整服务端的代码。
        /// </summary>
        public virtual void ComputeHash()
        {
            // TODO:
            var content = $"{OwnerId}{Url}{Method}{Body}{Cookie}";
            Hash = content.ToMd5();
        }
    }
}
using System;

namespace DotnetSpider.Proxy
{
    public class HttpProxy
    {
        public string Uri { get; private set; }

        /// <summary>
        /// 使用此代理下载数据的失败次数
        /// </summary>
        public int FailedNum { get; set; }

        public HttpProxy(string uri)
        {
            Uri = new Uri(uri).ToString();
        }
    }
}
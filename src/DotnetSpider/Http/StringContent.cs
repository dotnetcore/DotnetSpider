using System;


namespace DotnetSpider.Http
{
    [Serializable]
    public class StringContent : RequestContent
    {
        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; private set; }

        /// <summary>
        /// 编码
        /// </summary>
        public string EncodingName { get; set; }

        /// <summary>
        /// 内容类型
        /// </summary>
        public string MediaType { get; set; }

        public StringContent()
        {
        }

        public StringContent(string content, string mediaType = "text/plain", string encoding = "UTF-8")
        {
            Content = content;
            EncodingName = encoding;
            MediaType = mediaType;
        }
    }
}
using System.Collections.Generic;
using DotnetSpider.Infrastructure;
using MessagePack;

namespace DotnetSpider.Http
{
    public abstract class RequestContent
    {
        /// <summary>
        /// Headers
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        public void SetHeader(string header, string value)
        {
            header.NotNullOrWhiteSpace(nameof(header));
            value.NotNullOrWhiteSpace(nameof(value));

            if (Headers.ContainsKey(header))
            {
                Headers[header] = value.Trim();
            }
            else
            {
                Headers.Add(header, value.Trim());
            }
        }

        public string GetHeader(string header)
        {
            header.NotNullOrWhiteSpace(nameof(header));
            return Headers.ContainsKey(header) ? Headers[header] : null;
        }

        public virtual byte[] ToBytes()
        {
            return MessagePackSerializer.Typeless.Serialize(this);
        }
    }
}
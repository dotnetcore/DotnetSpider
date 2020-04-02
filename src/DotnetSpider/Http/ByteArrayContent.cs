using System;


namespace DotnetSpider.Http
{
    [Serializable]
    public class ByteArrayContent : RequestContent
    {
        /// <summary>
        /// 内容
        /// </summary>
        public byte[] Bytes { get; set; }

        public ByteArrayContent()
        {
        }

        public ByteArrayContent(byte[] bytes)
        {
            Bytes = bytes;
        }
    }
}
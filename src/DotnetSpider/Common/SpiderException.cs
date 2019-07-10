using System;

namespace DotnetSpider.Common
{
    public class SpiderException : Exception
    {
        public SpiderException(string msg) : base(msg)
        {
        }
    }
}
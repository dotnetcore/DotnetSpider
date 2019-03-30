using System;

namespace DotnetSpider.Core
{
    public class SpiderException : Exception
    {
        public SpiderException(string msg) : base(msg)
        {
        }
    }
}
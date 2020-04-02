using System;
using System.Text;

namespace DotnetSpider.Extensions
{
    public static class StringExtensions
    {
        public static string ToBase64String(this string value)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        }
    }
}
using DotnetSpider.Core;
using System;

namespace DotnetSpider.Extension.Redial
{
	public class RedialException : SpiderException
	{
		public RedialException(string message) : base(message)
		{
		}
	}
}

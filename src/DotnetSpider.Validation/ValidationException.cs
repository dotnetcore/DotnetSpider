using System;

namespace DotnetSpider.Validation
{
	public class ValidationException : Exception
	{
		public ValidationException(string msg) : base(msg)
		{
			
		}
	}
}

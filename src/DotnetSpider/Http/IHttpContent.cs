using System;

namespace DotnetSpider.Http
{
	public interface IHttpContent : IDisposable, ICloneable
	{
		ContentHeaders Headers { get; }
	}
}

using System;

namespace DotnetSpider.Core.Downloader
{
	public class DownloadException : Exception
	{
		public DownloadException(string message) : base(message)
		{
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Java2Dotnet.Spider.Core.Downloader
{
	public class DownloadException : Exception
	{
		public DownloadException(string message) : base(message)
		{
		}
	}
}

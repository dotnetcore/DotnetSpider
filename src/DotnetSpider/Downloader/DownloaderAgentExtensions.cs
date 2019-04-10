using System;
using DotnetSpider.Downloader.Entity;

namespace DotnetSpider.Downloader
{
	public static class DownloaderAgentExtensions
	{
		public static bool IsActive(this DownloaderAgent agent)
		{
			return (DateTime.Now - agent.LastModificationTime).TotalSeconds <= 12;
		}
	}
}
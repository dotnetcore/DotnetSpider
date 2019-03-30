using System;

namespace DotnetSpider.Downloader.Entity
{
    public class DownloaderAgentHeartbeat
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public int FreeMemory { get; set; }

        public int DownloaderCount { get; set; }

        public DateTime LastModificationTime { get; set; }
    }
}